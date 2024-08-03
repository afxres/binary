#!/bin/sh

cd code
dotnet restore
dotnet build
dotnet test -f net8.0 --collect:"XPlat Code Coverage;Format=lcov"
