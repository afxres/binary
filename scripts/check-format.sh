#!/usr/bin/env bash
# Format checks for the three languages in the repository.
#
#   csharp    dotnet format: analyzers and whitespace for the C# projects
#   fsharp    fantomas: the F# sources, tool version pinned in .config/dotnet-tools.json
#   projects  format-projects.cs: csproj / fsproj / props / targets and slnx as
#             XML. dotnet format refuses to look at F# projects, so this is the
#             only check that covers the fsproj files.
#
# Usage:
#   scripts/check-format.sh                 # check every stage
#   scripts/check-format.sh --csharp        # or a subset
#   scripts/check-format.sh --fsharp --projects
#   scripts/check-format.sh --fix           # rewrite files instead of checking
#
# Exit code: 0 clean, 1 needs formatting, 2 tool missing or command failed.

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_ROOT"

STAGES=()
FIX=0

for arg in "$@"; do
    case "$arg" in
        --csharp | --fsharp | --projects) STAGES+=("${arg#--}") ;;
        --fix) FIX=1 ;;
        -h | --help)
            sed -n '2,16p' "${BASH_SOURCE[0]}"
            exit 0
            ;;
        *)
            echo "error: unknown argument: $arg" >&2
            exit 2
            ;;
    esac
done

if [ "${#STAGES[@]}" -eq 0 ]; then
    STAGES=(csharp fsharp projects)
fi

note() {
    printf '\n=== %s ===\n' "$*"
}

require_dotnet() {
    if ! command -v dotnet >/dev/null; then
        echo 'error: dotnet not found' >&2
        return 2
    fi
}

check_csharp() {
    require_dotnet || return 2
    note "csharp: dotnet format $(dotnet format --version 2>/dev/null | head -1)"
    dotnet restore code/Binary.slnx
    local args=(code/Binary.slnx --severity warn --no-restore)
    if [ "$FIX" -eq 0 ]; then
        args+=(--verify-no-changes)
    fi
    dotnet format "${args[@]}"
}

check_fsharp() {
    require_dotnet || return 2
    note 'fsharp: fantomas (restored from .config/dotnet-tools.json)'
    dotnet tool restore
    if [ "$FIX" -eq 1 ]; then
        dotnet fantomas .
        return 0
    fi

    # fantomas reports unformatted files with exit code 99, anything else is a real error
    local code=0
    dotnet fantomas check . || code=$?
    if [ "$code" -eq 99 ]; then
        echo 'error: some F# sources are not formatted, run: scripts/check-format.sh --fsharp --fix' >&2
        return 1
    fi
    return "$code"
}

check_projects() {
    require_dotnet || return 2

    local files=()
    mapfile -t files < <(
        find "$PROJECT_ROOT/code" -type f \
            \( -name '*.csproj' -o -name '*.fsproj' -o -name '*.props' -o -name '*.targets' -o -name '*.slnx' \) \
            -not -path '*/obj/*' -not -path '*/bin/*' -not -path '*/.vs/*' | sort
    )
    if [ "${#files[@]}" -eq 0 ]; then
        echo 'error: no project file found under code/' >&2
        return 2
    fi

    local mode=()
    if [ "$FIX" -eq 1 ]; then
        mode+=(--fix)
    fi

    note "projects: xml layout over ${#files[@]} files"
    dotnet run "$PROJECT_ROOT/scripts/format-projects.cs" -- "${mode[@]}" "${files[@]}"
}

status=0
for stage in "${STAGES[@]}"; do
    "check_$stage" || status=1
done
exit "$status"
