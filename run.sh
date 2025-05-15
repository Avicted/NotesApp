#!/bin/bash

set -xe
 
dotnet clean
dotnet restore
dotnet run --project "NotesApp.Web/NotesApp.Web.csproj"
