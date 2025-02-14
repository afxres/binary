#!/bin/bash

PROJECT_ROOT="$PWD"
PROJECT_RESULT_DIR="$PROJECT_ROOT/build-result"
COVERAGE_FILE_PATH="$PROJECT_RESULT_DIR/lcov.info"

mkdir -p "$PROJECT_RESULT_DIR"
find . -name coverage.info -exec echo -a {} \; | xargs lcov --output-file "$COVERAGE_FILE_PATH"
