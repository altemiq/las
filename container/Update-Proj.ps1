$BaseUri = 'https://download.osgeo.org/osgeo4w/v2'
$OutputDirectory = Join-Path $PSScriptRoot PROJ
$TempDirectory = Join-Path $env:TEMP update-proj
if (-not (Test-Path $TempDirectory -PathType Container)) {
    New-Item $TempDirectory -ItemType Directory
}

$SetupIni = Join-Path $TempDirectory setup.ini
if (-not (Test-Path $SetupIni -PathType Leaf)) {
    $SetupIniUri = "$BaseUri/x86_64/setup.ini"
    Invoke-WebRequest -Uri $SetupIniUri -OutFile $SetupIni
}

# Read setup.ini
$Content = Get-Content $SetupIni

# Find the latest proj-runtime-data package entry
$PackageName = 'proj-runtime-data'

$PackageLine = $null
for ($i = 0; $i -lt $Content.Count; $i++) {
    if ($Content[$i] -eq "@ $PackageName") {
        for ($j = $i; $j -lt $Content.Count; $j++) {
            if ($Content[$j] -match '^install:\s+(\S+)') {
                $PackageLine = $Matches[1]
                break
            }
        }
        break
    }
}

if (-not $PackageLine) {
    throw "Could not locate package '$PackageName' in setup.ini"
}

# Download package
$PackagePath = $PackageLine.Split()[0]
$PackageFile = Join-Path $TempDirectory (Split-Path $PackagePath -Leaf)
if (-not (Test-Path $PackageFile -PathType Leaf)) {
    $PackageUri = "$BaseUri/$PackagePath"
    Invoke-WebRequest -Uri $PackageUri -OutFile $PackageFile
}

$ExtractedDirectory = Join-Path $TempDirectory extract

tar -xzf $PackageFile -C $ExtractedDirectory

$ProjDataDirectory = Join-Path $ExtractedDirectory share proj

Get-ChildItem $ProjDataDirectory -Filter proj* | ForEach-Object {
    Copy-Item $_.FullName -Destination $(Join-Path $OutputDirectory $_.Name)
}

Remove-Item $TempDirectory -Recurse -Force