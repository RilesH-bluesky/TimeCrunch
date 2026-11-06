# Runs the whole NUnit suite with code coverage and builds an HTML summary.
# Usage: .\run-tests.ps1     (from the folder that holds TimeCrunch.slnx)
$ErrorActionPreference = "Stop"
$results = "TestResults"
if (Test-Path $results) { Remove-Item $results -Recurse -Force }

dotnet test TimeCrunch.Tests `
  --collect:"XPlat Code Coverage" `
  --settings coverlet.runsettings `
  --results-directory $results `
  --logger "trx;LogFileName=results.trx"

# One-time install:  dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator `
  -reports:"$results/**/coverage.cobertura.xml" `
  -targetdir:"$results/coverage-report" `
  -reporttypes:"Html;TextSummary;Badges"

Write-Host ""
Get-Content "$results/coverage-report/Summary.txt"
Write-Host ""
Write-Host "Full report: $results/coverage-report/index.html"
