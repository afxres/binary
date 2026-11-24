#!/usr/bin/env bash
# Download IBM's .NET SDK build for s390x (an unofficial port of the official
# SDK) into a directory.
#
# Usage:
#   scripts/install-s390x.sh                            # newest 11.x build, /opt/dotnet-s390x
#   scripts/install-s390x.sh --dir /tmp/sdk             # somewhere else
#   scripts/install-s390x.sh --tag v11.0.100-rc.2.26504.105 --force
#
# The 'latest' release of the repository is not what we want: it still tracks
# the previous major line, so the tag is resolved as the newest release whose
# major version matches --major (the major the projects target).
#
# Needs curl and jq on the host, and the s390x system libraries plus the
# binfmt handler installed by scripts/setup-s390x.sh for the self check at the
# end (the s390x dotnet is exec'd through qemu-user). Extraction is skipped
# when a working SDK is already in place, which keeps the cached directory in
# CI cheap on re-runs.
#
# The release lookup goes through the GitHub API, which allows only 60
# anonymous requests per hour and IP. CI runners share their egress addresses,
# so set GH_TOKEN to authenticate the lookup and stay clear of that limit.

set -euo pipefail

DIR=/opt/dotnet-s390x
MAJOR=11
TAG=""
FORCE=0

while [ "$#" -gt 0 ]; do
    case "$1" in
        --dir)
            DIR="${2:?--dir needs a value}"
            shift 2
            ;;
        --tag)
            TAG="${2:?--tag needs a value}"
            shift 2
            ;;
        --major)
            MAJOR="${2:?--major needs a value}"
            shift 2
            ;;
        --dir=*)
            DIR="${1#*=}"
            shift
            ;;
        --tag=*)
            TAG="${1#*=}"
            shift
            ;;
        --major=*)
            MAJOR="${1#*=}"
            shift
            ;;
        --force)
            FORCE=1
            shift
            ;;
        -h | --help)
            sed -n '2,22p' "${BASH_SOURCE[0]}"
            exit 0
            ;;
        *)
            echo "error: unknown argument: $1" >&2
            exit 2
            ;;
    esac
done

if [ -x "$DIR/dotnet" ] && [ "$FORCE" -eq 0 ]; then
    echo "s390x SDK already installed in $DIR: $("$DIR/dotnet" --version)"
    exit 0
fi

for tool in curl jq; do
    if ! command -v "$tool" >/dev/null; then
        echo "error: $tool not found (apt-get install -y $tool)" >&2
        exit 2
    fi
done

API=https://api.github.com/repos/IBM/dotnet-s390x
RELEASES=$API/releases

AUTH=()
if [ -n "${GH_TOKEN:-}" ]; then
    AUTH=(-H "Authorization: Bearer $GH_TOKEN")
fi

# Print the status code instead of letting 'curl -f' abort with exit code 22:
# a 403 from the anonymous rate limit is the failure worth explaining.
api_get() {
    local url="$1" response status
    if ! response="$(curl -sSL --retry 3 --retry-all-errors ${AUTH[@]+"${AUTH[@]}"} -w '\n%{http_code}' "$url")"; then
        echo "error: cannot reach $url" >&2
        exit 2
    fi
    status="${response##*$'\n'}"
    if [ "$status" != "200" ]; then
        echo "error: $url returned HTTP $status" >&2
        if [ "$status" = "403" ]; then
            echo "hint: anonymous GitHub API requests are capped at 60 per hour and IP," >&2
            echo "      export GH_TOKEN to authenticate the release lookup" >&2
        fi
        exit 2
    fi
    printf '%s\n' "${response%$'\n'*}"
}

if [ -z "$TAG" ]; then
    echo "resolving the newest $MAJOR.x s390x release ..." >&2
    # the lookup is kept out of the pipeline: 'jq' succeeds on empty input, so
    # a failed request would otherwise be reported as "no release found"
    releases="$(api_get "$RELEASES?per_page=100")" || exit 2
    TAG="$(printf '%s\n' "$releases" | jq -r --arg major "v$MAJOR." '
        [ .[] | select(.tag_name | startswith($major)) ]
        | sort_by(.published_at)
        | last
        | .tag_name // empty')"
    if [ -z "$TAG" ]; then
        echo "error: no $MAJOR.x release found, pick one with --tag" >&2
        exit 2
    fi
fi

release="$(api_get "$RELEASES/tags/$TAG")" || exit 2
ASSET="$(printf '%s\n' "$release" | jq -r '.assets[].name' | grep -E '^dotnet-sdk-.*-linux-s390x\.tar\.gz$' | head -1 || true)"
if [ -z "$ASSET" ]; then
    echo "error: release $TAG has no dotnet-sdk-*-linux-s390x.tar.gz asset" >&2
    exit 2
fi

echo "installing $ASSET ($TAG) into $DIR"
mkdir -p "$DIR"
ARCHIVE="$(mktemp)"
trap 'rm -f "$ARCHIVE"' EXIT
ASSET_URL="https://github.com/IBM/dotnet-s390x/releases/download/$TAG/$ASSET"
if ! curl -fsSL --retry 3 --retry-all-errors -o "$ARCHIVE" "$ASSET_URL"; then
    echo "error: cannot download $ASSET_URL" >&2
    exit 2
fi
tar -xzf "$ARCHIVE" -C "$DIR"

echo "s390x SDK $TAG installed: $("$DIR/dotnet" --version)"
