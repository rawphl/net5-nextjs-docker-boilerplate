#!/bin/bash

export PATH="$PATH:/root/.dotnet/tools"

dotnet ef database update

dotnet run --no-build --urls http://0.0.0.0:5000 -v d
