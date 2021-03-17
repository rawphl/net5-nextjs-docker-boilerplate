#!/bin/bash

export PATH="$PATH:/root/.dotnet/tools"

dotnet ef database update

dotnet watch run --no-restore --urls http://0.0.0.0:5000 -v d
