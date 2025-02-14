#!/bin/bash

PROJECT_ROOT="$PWD"
PROJECT_DIR="$PROJECT_ROOT/code"

cd "$PROJECT_DIR"
dotnet restore
dotnet build
dotnet test --collect:"XPlat Code Coverage;Format=lcov"
