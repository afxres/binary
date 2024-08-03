#!/bin/sh

sudo apt-get install -y lcov
mkdir -p coverage
find . -name coverage.info -exec echo -a {} \; | xargs lcov -o coverage/lcov.info
