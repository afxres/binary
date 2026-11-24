#!/bin/bash
#
# Check the shell scripts under scripts/: syntax, formatting, lint.
#
#   bash scripts/check-shell.sh          # report, exit 1 on any finding
#   bash scripts/check-shell.sh --fix    # rewrite the files with shfmt
#
# Formatting is `shfmt` (mvdan.cc/sh), the shell equivalent of gofmt: it parses
# the script and prints it again, so it normalises case-arm indentation and
# line continuations that review never catches consistently. The style itself
# is declared once in .editorconfig, so editors and this script agree.
#
# `bash -n` catches what shfmt cannot repair, and `shellcheck` catches what
# neither can see (unquoted expansions, `cd` without a guard). shellcheck is
# optional: without it the check still runs and says so.

set -uo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.." || exit 1

FIX=0
case "${1:-}" in
    --fix) FIX=1 ;;
    -h | --help)
        sed -n '2,15p' "${BASH_SOURCE[0]}" | sed -e 's/^# \{0,1\}//'
        exit 0
        ;;
    "") ;;
    *)
        echo "usage: $0 [--fix]" >&2
        exit 2
        ;;
esac

mapfile -t files < <(find scripts -type f -name '*.sh' | sort)
if [ "${#files[@]}" -eq 0 ]; then
    echo "no shell scripts found in scripts/" >&2
    exit 1
fi

status=0
for file in "${files[@]}"; do
    bash -n "$file" || status=1
    if [ "$FIX" -eq 1 ]; then
        shfmt -w "$file"
    elif ! shfmt -d "$file"; then
        echo "  ^ $file: run '$0 --fix' to reformat"
        status=1
    fi
done

if command -v shellcheck >/dev/null 2>&1; then
    shellcheck -S warning "${files[@]}" || status=1
else
    echo "note: shellcheck not installed, lint skipped (apt install shellcheck)"
fi

if [ "$status" -eq 0 ]; then
    printf '\n%s scripts clean\n' "${#files[@]}"
fi
exit "$status"
