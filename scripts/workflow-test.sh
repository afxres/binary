#!/bin/sh

cd code
dotnet restore
dotnet build
dotnet test --collect:"XPlat Code Coverage;Format=lcov"
