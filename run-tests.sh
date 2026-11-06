#!/usr/bin/env bash
# Runs the whole NUnit suite with code coverage and builds an HTML summary.
# Usage: ./run-tests.sh            (from the folder that holds TimeCrunch.slnx)
set -euo pipefail

RESULTS="TestResults"
rm -rf "$RESULTS"

dotnet test TimeCrunch.Tests \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings \
  --results-directory "$RESULTS" \
  --logger "trx;LogFileName=results.trx"

# One-time install:  dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:"$RESULTS/**/coverage.cobertura.xml" \
  -targetdir:"$RESULTS/coverage-report" \
  -reporttypes:"Html;TextSummary;Badges"

echo
cat "$RESULTS/coverage-report/Summary.txt"
echo
echo "Full report: $RESULTS/coverage-report/index.html"
