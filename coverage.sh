#!/bin/bash

# PURPOSE: Automates the running of Unit Tests and Code Coverage
# REF: https://stackoverflow.com/a/70321555/495455

# If running outside the test folder
#cd E:\Dev\XYZ\src\XYZTestProject

# This only needs to be installed once (globally), if installed it fails silently: 
dotnet tool install -g dotnet-reportgenerator-globaltool

# Save directories into a variables
baseDir=$(dirname "$0")
coverageDir=$(realpath $baseDir)/coverage
resultsDir=$coverageDir/results
historyDir=$coverageDir/history
reportDir=$coverageDir/report

# Delete previous test run results (there's a bunch of subfolders named with guids)
if [ -d $resultsDir ]; then
  echo "Removing $resultsDir" 
  rm -rf $resultsDir
fi

# Run the tests
dotnet test --solution $baseDir --results-directory $resultsDir --coverage --coverage-output-format cobertura

# Delete previous test run reports - note if you're getting wrong results do a Solution Clean and Rebuild to remove stale DLLs in the bin folder
if [ -d $reportDir ]; then
  echo "Removing $reportDir"
  rm -rf $reportDir
fi

# Generate the Code Coverage HTML Report
reportgenerator -reports:"$resultsDir/*.cobertura.xml" -targetdir:$reportDir -reporttypes:Html -historydir:$historyDir
