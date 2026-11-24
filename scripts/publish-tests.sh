#!/usr/bin/env bash
# Publish every test project as a payload that can be executed on a machine of
# the other byte order.
#
# Usage:
#   scripts/publish-tests.sh <output directory> [--framework <tfm>]
#       [--runtime-version <version>] [--only <name filter>]...
#
# The name filters are substrings matched against the project paths; a project
# is published when it matches any of them. They exist for the s390x job,
# which runs a byte order spot check on two suites instead of the full set
# that the x86-64 job already covers.
#
# The optional runtime version is written into every generated
# *.runtimeconfig.json after publishing. It exists because the s390x SDK is
# still a release candidate: without pinning, a payload built by a GA SDK asks
# for 11.0.0 and refuses to start on the 11.0.0-rc runtime that IBM ships.
# It cannot be a build property: apphost packs are published for GA versions
# only, so RuntimeFrameworkVersion makes the publish fail to produce
# obj/.../apphost while the copy step still asks for it.
#
# The app host stays enabled: xUnit v3 rejects a test project that builds
# without one. It is unused on the other side of the crossing, where the
# assembly is started with 'dotnet exec'.

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="${1:?usage: publish-tests.sh <output directory> [--framework <tfm>] [--runtime-version <version>] [--only <name filter>]...}"
shift

FRAMEWORK=net11.0
RUNTIME_VERSION=
FILTERS=()
while [ "$#" -gt 0 ]; do
    case "$1" in
        --framework)
            FRAMEWORK="${2:?--framework needs a value}"
            shift 2
            ;;
        --runtime-version)
            RUNTIME_VERSION="${2:?--runtime-version needs a value}"
            shift 2
            ;;
        --only)
            FILTERS+=("${2:?--only needs a value}")
            shift 2
            ;;
        *)
            echo "error: unknown argument: $1" >&2
            exit 2
            ;;
    esac
done

cd "$PROJECT_ROOT"
mkdir -p "$OUT"

# Test project paths of the solution, one per line, relative to the repository
# root. Benchmark projects are excluded on purpose: several of them are named
# '...Tests' as well. The list is derived instead of written down by hand, so
# no caller has to be updated when a test project is added or removed.
list_test_projects() {
    # 'dotnet sln list' prints paths relative to the solution, so they are
    # prefixed with the directory holding it to make them usable from the root.
    dotnet sln code/Binary.slnx list | tr -d '\r' | sed 's|\\|/|g' | grep -E '/Binary[A-Za-z.]*Tests[A-Za-z.]*\.(cs|fs)proj$' | sed 's|^|code/|'
}

projects=()
while read -r project; do
    if [ "${#FILTERS[@]}" -gt 0 ]; then
        match=0
        for filter in "${FILTERS[@]}"; do
            case "$project" in *"$filter"*)
                match=1
                break
                ;;
            esac
        done
        [ "$match" -eq 1 ] || continue
    fi
    projects+=("$project")
done < <(list_test_projects)

if [ "${#projects[@]}" -eq 0 ]; then
    echo "error: no test project matches: ${FILTERS[*]}" >&2
    exit 2
fi

for project in "${projects[@]}"; do
    echo "== $project"
    dotnet publish "$project" \
        --framework "$FRAMEWORK" \
        --configuration Debug \
        --output "$OUT"
done

if [ -n "$RUNTIME_VERSION" ]; then
    pinned=0
    for config in "$OUT"/*.runtimeconfig.json; do
        if [ ! -e "$config" ]; then
            continue
        fi
        sed -i -E "s/(\"version\"[[:space:]]*:[[:space:]]*\")[^\"]*(\")/\1$RUNTIME_VERSION\2/" "$config"
        pinned=$((pinned + 1))
    done
    if [ "$pinned" -eq 0 ]; then
        echo 'error: no runtimeconfig.json to pin in '"$OUT" >&2
        exit 2
    fi
    echo "pinned $pinned payload(s) to $RUNTIME_VERSION"
fi

echo
echo "published into $OUT"
