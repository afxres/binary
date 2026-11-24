#!/usr/bin/env bash
# Run one command with the s390x dotnet on PATH.
#
# Usage: scripts/run-on-s390x.sh 'bash scripts/publish-tests.sh ...'
#
# The s390x ELF binaries are dispatched to qemu-user by the kernel binfmt
# handlers (scripts/setup-s390x.sh installs the handler and the s390x system
# libraries), so no container is involved. The SDK itself lives in
# build-result/sdk-s390x, kept across steps of one run.
#
# The projects target net11.0 while the s390x SDK is an 11.x build, so the
# runtime is reached through roll forward; without the prerelease flag an
# 11.0.0-rc runtime is not considered at all.

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SDK_DIR="${S390X_SDK:-$PROJECT_ROOT/build-result/sdk-s390x}"
COMMAND="${1:?usage: run-on-s390x.sh <command>}"

if [ ! -x "$SDK_DIR/dotnet" ]; then
    echo "error: the s390x SDK is not installed in $SDK_DIR" >&2
    echo 'run scripts/install-s390x.sh first' >&2
    exit 2
fi

export DOTNET_ROOT="$SDK_DIR"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export DOTNET_ROLL_FORWARD=LatestMajor
export DOTNET_ROLL_FORWARD_TO_PRERELEASE=1
export PATH="$SDK_DIR:$PATH"

eval "$COMMAND"
