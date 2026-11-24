#!/usr/bin/env bash
# Install the system libraries that the s390x dotnet binaries need and make
# sure the kernel dispatches s390x ELF files to qemu-user.
#
# The s390x SDK tarball carries only managed code and the dotnet host. It links
# against the s390x variants of libc, libssl, libicu and zlib, which an x86-64
# host does not have. Ubuntu's multiarch support installs them next to the
# native ones, and a binfmt handler then lets the kernel exec the s390x
# binaries directly, without a container.
#
# The hosted runners ship no binfmt handler and the static emulator lives in
# the qemu-user package these days while qemu-user-static is symlink-only, so
# the entry is written by hand after extracting qemu-s390x from that package.
# The emulator from tonistiigi/binfmt was tried first and drops guest argv[0],
# which leaves the dotnet host printing its usage text.
#
# apt is deliberately scoped: a full update pulls every index for both
# architectures (about 80 MB) while this script needs about 20, and the
# qemu-user package carries forty emulators when one is used. The sources are
# isolated for the update only, so nothing on the host is rewritten.
#
# Usage: sudo scripts/setup-s390x.sh
set -euo pipefail

if [ "$(id -u)" -ne 0 ]; then
    echo "error: run with sudo" >&2
    exit 2
fi

if [ "$(dpkg --print-architecture)" != "amd64" ]; then
    echo "error: this script assumes an amd64 host" >&2
    exit 2
fi

ENTRY=/proc/sys/fs/binfmt_misc/qemu-s390x
# ELF64 MSB with e_machine 22 (s390x); the mask leaves only class, endianness
# and machine significant
MAGIC=7f454c4602020100000000000000000000020016
MASK=fffffffffffffffcfffffffffffffffffffeffff

if [ ! -e /proc/sys/fs/binfmt_misc ]; then
    mount -t binfmt_misc binfmt_misc /proc/sys/fs/binfmt_misc
fi

dpkg --add-architecture s390x

# the indexes this script actually needs: qemu-user from the amd64 universe,
# the s390x libraries from the ports main component. The runner ships indexes
# for the amd64 suites already, but stale ones are a 404 risk on download, so
# the amd64 part is refreshed here as well.
cat >/tmp/setup-s390x.sources <<'EOF'
Types: deb
URIs: http://archive.ubuntu.com/ubuntu/
Suites: noble
Components: main universe
Architectures: amd64
Signed-By: /usr/share/keyrings/ubuntu-archive-keyring.gpg

Types: deb
URIs: http://ports.ubuntu.com/ubuntu-ports/
Suites: noble noble-updates noble-security
Components: main restricted
Architectures: s390x
Signed-By: /usr/share/keyrings/ubuntu-archive-keyring.gpg
EOF

echo "[$(date +%s)] updating the isolated indexes ..." >&2
# the same options repeat on every apt call below: the candidate cache is
# built from the sources visible to that call, so the isolated list must stay
# in effect or the s390x packages become unlocatable again
APT_OPTIONS=(
    -o Dir::Etc::sourcelist=/tmp/setup-s390x.sources
    -o Dir::Etc::sourceparts=-
)
apt-get update -qq "${APT_OPTIONS[@]}" >/dev/null

echo "[$(date +%s)] installing the s390x libraries ..." >&2
# libc6 is reached through /lib/ld64.so.1, libicu through the version probe in
# the runtime, libssl through the handshake probe. libstdc++6 and zlib1g come
# with the native pack of the SDK; apt resolves libgcc-s1 and friends.
apt-get install -y -qq --no-install-recommends "${APT_OPTIONS[@]}" \
    libc6:s390x libstdc++6:s390x zlib1g:s390x libssl3t64:s390x libicu74:s390x \
    >/dev/null

echo "[$(date +%s)] extracting qemu-s390x ..." >&2
# the whole package is forty emulators, one binary is used: download the deb
# and unpack only what is needed instead of installing it. apt-get download
# writes to the current directory, so work in a scratch one.
WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT
cd "$WORK"
apt-get download -q "${APT_OPTIONS[@]}" qemu-user >/dev/null
dpkg-deb -x qemu-user_*.deb "$WORK/pkg"
cp "$WORK/pkg/usr/bin/qemu-s390x" /usr/local/bin/qemu-s390x

if [ ! -e "$ENTRY" ]; then
    echo "[$(date +%s)] registering the s390x binfmt handler ..." >&2
    printf ':qemu-s390x:M::%s:%s:%s:FP\n' \
        "$(printf '%s' "$MAGIC" | sed -e 's/../\\x&/g')" \
        "$(printf '%s' "$MASK" | sed -e 's/../\\x&/g')" \
        /usr/local/bin/qemu-s390x >/proc/sys/fs/binfmt_misc/register
fi
if [ ! -e "$ENTRY" ]; then
    echo 'error: the s390x binfmt handler is missing' >&2
    exit 2
fi

echo "[$(date +%s)] s390x system libraries installed:"
ls /lib/s390x-linux-gnu/libc.so.6 /lib/s390x-linux-gnu/libssl.so.3 /lib/s390x-linux-gnu/libicui18n.so.74
