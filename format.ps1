dotnet format whitespace $PSScriptRoot --report $(Join-Path $PSScriptRoot whitespace.report.json) --verbosity diagnostic --no-restore
dotnet format style $PSScriptRoot  --report $(Join-Path $PSScriptRoot style.report.json) --severity info --verbosity diagnostic --no-restore
dotnet format analyzers $PSScriptRoot --report $(Join-Path $PSScriptRoot analyzers.report.json) --severity info --verbosity diagnostic --exclude-diagnostics S1133 --no-restore
