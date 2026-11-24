#!/usr/bin/env bash
# Execute every published test payload in a directory.
#
# Each *.runtimeconfig.json in the payload is one test application, so the list
# comes from the output itself instead of a hard coded project list.
#
# Usage: scripts/run-published-tests.sh <published directory> [jobs]
#
# 'jobs' defaults to 1. Emulated runs pass the cpu count instead: the payloads
# are separate processes that share nothing, and every one of them is bound to
# a single core, so the wall time drops almost linearly. The output is captured
# per application and printed in the original order once everything finished,
# which keeps the log readable and the failure list deterministic.

set -uo pipefail

DIR="${1:?usage: run-published-tests.sh <published directory> [jobs]}"
JOBS="${2:-1}"

shopt -s nullglob

echo "== $(uname -m)"
dotnet --list-runtimes | grep Microsoft.NETCore.App

applications=("$DIR"/*.runtimeconfig.json)
if [ "${#applications[@]}" -eq 0 ]; then
    echo "error: no test application found in $DIR" >&2
    exit 2
fi

failed=()

if [ "$JOBS" -le 1 ]; then
    for runtimeconfig in "${applications[@]}"; do
        dll="${runtimeconfig%.runtimeconfig.json}.dll"
        echo
        echo "== $(basename "$dll")"
        # the app host belongs to the machine that built the payload, so the
        # assembly is started explicitly instead
        dotnet exec "$dll" || failed+=("$(basename "$dll")")
    done
else
    logs="$(mktemp -d)"
    trap 'rm -rf "$logs" > /dev/null 2>&1' EXIT

    names=()
    running=0
    for index in "${!applications[@]}"; do
        dll="${applications[$index]%.runtimeconfig.json}.dll"
        name="$(basename "$dll")"
        names+=("$name")
        (
            {
                echo
                echo "== $name"
                dotnet exec "$dll"
            } >"$logs/$index.log" 2>&1
            echo "$?" >"$logs/$index.status"
        ) &
        running=$((running + 1))
        if [ "$running" -ge "$JOBS" ]; then
            wait -n
            running=$((running - 1))
        fi
    done
    wait

    for index in "${!names[@]}"; do
        cat "$logs/$index.log"
        if [ "$(cat "$logs/$index.status" 2>/dev/null)" != 0 ]; then
            failed+=("${names[$index]}")
        fi
    done
fi

if [ "${#failed[@]}" -gt 0 ]; then
    echo
    echo "error: failed: ${failed[*]}" >&2
    exit 1
fi

echo
echo 'all published test applications passed'
