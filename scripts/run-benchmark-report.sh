#!/bin/bash

PROJECT_ROOT="$PWD"
PROJECT_RESULT_DIR="$PROJECT_ROOT/build-result"
BENCHMARK_RESULT_FILE_PATH="$PROJECT_RESULT_DIR/benchmark-result.json"
BENCHMARK_OUTPUT_FILE_NAME="Mikodev.Binary.Benchmarks.IntegrationTests.IntegrationBenchmarks-report-full-compressed.json"
BENCHMARK_EXE="$PROJECT_ROOT/code/Benchmarks.IntegrationTests/bin/Release/net9.0/Mikodev.Binary.Benchmarks.IntegrationTests"

if [ ! -f "$BENCHMARK_EXE" ]; then
    echo "benchmark not found"
    exit 1
fi

mkdir -p "$PROJECT_RESULT_DIR"
cd "$(dirname "$BENCHMARK_EXE")" && "$BENCHMARK_EXE" && cd "$PROJECT_ROOT"
find . -type f | grep -F "$BENCHMARK_OUTPUT_FILE_NAME" | xargs --no-run-if-empty -I {} cp "{}" "$BENCHMARK_RESULT_FILE_PATH"
