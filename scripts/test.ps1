# PowerShell script to run tests with coverage

param(
    [string]$Configuration = "Release",
    [int]$CoverageThreshold = 80
)

Write-Host "Running tests with coverage analysis..." -ForegroundColor Green

# Clean previous results
if (Test-Path "coverage") {
    Remove-Item -Recurse -Force "coverage"
}

# Run tests with coverage
Write-Host "Running unit tests..." -ForegroundColor Yellow
dotnet test --configuration $Configuration --collect:"XPlat Code Coverage" --results-directory ./coverage --logger trx --logger "console;verbosity=normal"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Install report generator if not already installed
Write-Host "Installing report generator..." -ForegroundColor Yellow
dotnet tool install -g dotnet-reportgenerator-globaltool --version 5.2.0

# Generate coverage report
Write-Host "Generating coverage report..." -ForegroundColor Yellow
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html;Cobertura;OpenCover"

# Check coverage threshold
Write-Host "Checking coverage threshold..." -ForegroundColor Yellow
$coverageFile = Get-ChildItem -Path "coverage" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -First 1

if ($coverageFile) {
    [xml]$coverageXml = Get-Content $coverageFile.FullName
    $lineRate = [double]$coverageXml.coverage.'line-rate'
    $coveragePercent = [math]::Round($lineRate * 100, 2)
    
    Write-Host "Coverage: $coveragePercent%" -ForegroundColor Cyan
    
    if ($coveragePercent -lt $CoverageThreshold) {
        Write-Host "Coverage is below threshold of $CoverageThreshold%!" -ForegroundColor Red
        exit 1
    } else {
        Write-Host "Coverage meets threshold of $CoverageThreshold%!" -ForegroundColor Green
    }
} else {
    Write-Host "Coverage file not found!" -ForegroundColor Red
    exit 1
}

Write-Host "All tests passed and coverage requirements met!" -ForegroundColor Green
Write-Host "Coverage report available at: coverage/report/index.html" -ForegroundColor Cyan
