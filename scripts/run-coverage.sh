#!/bin/bash
#
# Merge the coverage files of every test suite into build-result/lcov.info.
#
#   bash scripts/run-coverage.sh
#
# Run the tests first: every suite writes its own coverage*.info next to the
# binaries, and this script only merges what is already on disk.
#
# Requires lcov: apt install lcov.

set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.." || exit 1

PROJECT_ROOT="$PWD"
PROJECT_RESULT_DIR="$PROJECT_ROOT/build-result"
COVERAGE_FILE_PATH="$PROJECT_RESULT_DIR/lcov.info"

if ! command -v lcov >/dev/null 2>&1; then
    echo "lcov not installed: apt install lcov" >&2
    exit 1
fi

mkdir -p "$PROJECT_RESULT_DIR"
inputs=()
while IFS= read -r -d '' file; do
    inputs+=(-a "$file")
done < <(find . -name 'coverage*.info' -print0)
lcov --output-file "$COVERAGE_FILE_PATH" "${inputs[@]}"
