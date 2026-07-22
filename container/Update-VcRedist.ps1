# download the latest vs redist
$TempDirectory = Join-Path $env:TEMP update-vs-redist
if (-not (Test-Path $TempDirectory -PathType Container)) {
    New-Item $TempDirectory -ItemType Directory
}

$VcRedistDownload = Join-Path $TempDirectory VC_redist.x64.exe

if (-not (Test-Path $VcRedistDownload -PathType Leaf)) {
    Invoke-WebRequest -Uri https://aka.ms/vc14/vc_redist.x64.exe -OutFile $VcRedistDownload
}

$WixOutputDirectory = Join-Path $TempDirectory o
$WixOutputBADirectory = Join-Path $TempDirectory oba

dotnet dnx wix --version 6.0.2 -- burn extract $VcRedistDownload -o $WixOutputDirectory -oba $WixOutputBADirectory

# get lessmsi
$LessMsiVersion = 'v2.12.9'
$LessMsiDownload = Join-Path $TempDirectory lessmsi.zip
if (-not (Test-Path $LessMsiDownload -PathType Leaf)) {
    Invoke-WebRequest -Uri https://github.com/activescott/lessmsi/releases/download/$LessMsiVersion/lessmsi-$LessMsiVersion.zip -OutFile $LessMsiDownload
}

$LessMsiCli = Join-Path $TempDirectory lessmsi lessmsi.exe
if (-not (Test-Path $LessMsiCli -PathType Leaf)) {
    Expand-Archive $LessMsiDownload -DestinationPath $(Join-Path $TempDirectory lessmsi)
}

$OutputDirectory = Join-Path $PSScriptRoot VC_redist.x64

# extract any x64.msi files
Get-ChildItem -Path $WixOutputDirectory -Include *Minimum_x64.msi -Recurse | ForEach-Object {
    $MsiOutputDirectory = Join-Path $TempDirectory msi $_.BaseName
    if (-not (Test-Path $MsiOutputDirectory -PathType Container)) {
        New-Item $MsiOutputDirectory -ItemType Directory
    }

    # extract out the files
    & $LessMsiCli xo $_.FullName $MsiOutputDirectory\

    # copy the files to the output
    Get-ChildItem -Path $MsiOutputDirectory -Include *.dll -Recurse | ForEach-Object {
        $Destination = Join-Path $OutputDirectory $_.Name
        Copy-Item -Path $_.FullName -Destination $Destination
    }
}

Remove-Item $TempDirectory -Recurse -Force