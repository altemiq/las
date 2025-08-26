# PURPOSE: Automates the running of Unit Tests and Code Coverage
# REF: https://stackoverflow.com/a/70321555/495455

# If running outside the test folder
#cd E:\Dev\XYZ\src\XYZTestProject

# This only needs to be installed once (globally), if installed it fails silently: 
dotnet tool install -g dotnet-reportgenerator-globaltool

# Save directorys into a variables
$dir = pwd
$coverageDir = Join-Path $dir 'coverage'
$resultsDir = Join-Path $coverageDir 'results'
$historyDir = Join-Path $coverageDir 'history'
$reportDir = Join-Path $coverageDir 'report'

# Delete previous test run results (there's a bunch of subfolders named with guids)
if (Test-Path -path $resultsDir) { 
  Remove-Item -Recurse -Force $resultsDir
}

# Run the tests
dotnet test -- --results-directory $resultsDir --coverage --coverage-output-format cobertura

# Delete previous test run reports - note if you're getting wrong results do a Solution Clean and Rebuild to remove stale DLLs in the bin folder
if (Test-Path -path $reportDir) { 
  Remove-Item -Recurse -Force $reportDir
}

# To keep a history of the Code Coverage we need to use the argument: -historydir:SOME_DIRECTORY 
if (!(Test-Path -path $historyDir)) {  
  New-Item -ItemType directory -Path $historyDir
}

# Generate the Code Coverage HTML Report
reportgenerator -reports:"$resultsDir/**/coverage.cobertura.xml" -targetdir:$reportDir -reporttypes:Html -historydir:$historyDir

# Open the Code Coverage HTML Report (if running on a WorkStation)
$osInfo = Get-CimInstance -ClassName Win32_OperatingSystem
if ($osInfo.ProductType -eq 1) {
  (& "$reportDir/index.html")
}
