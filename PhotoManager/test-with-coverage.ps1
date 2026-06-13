param(
    [string] $Filter = '',
    [switch] $OpenReport
)

$ErrorActionPreference = 'Stop'

$solutionDirectory = $PSScriptRoot
$testResultsDirectory = Join-Path $solutionDirectory 'TestResults'
$testProjectResultsDirectory = [System.IO.Path]::Combine($solutionDirectory, 'PhotoManager.Tests', 'TestResults')
$toolsDirectory = Join-Path $solutionDirectory 'Tools'
$reportGeneratorName = if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::Windows))
{
    'reportgenerator.exe'
}
else
{
    'reportgenerator'
}
$reportGeneratorPath = Join-Path $toolsDirectory $reportGeneratorName

$coverageDirectories = @($testResultsDirectory, $testProjectResultsDirectory)
Remove-Item -LiteralPath $coverageDirectories -Recurse -Force -ErrorAction SilentlyContinue

$testArguments = @(
    'test',
    '/p:CollectCoverage=true',
    '/p:CoverletOutput=TestResults/',
    '/p:ExcludeByAttribute=GeneratedCodeAttribute',
    '/p:ExcludeByAttribute=ExcludeFromCodeCoverageAttribute',
    'PhotoManager.slnx',
    '--collect:XPlat Code Coverage'
)

if (![string]::IsNullOrWhiteSpace($Filter))
{
    $testArguments += @('--filter', $Filter)
}

Push-Location $solutionDirectory

try
{
    & dotnet @testArguments

    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }

    if (!(Test-Path -LiteralPath $reportGeneratorPath))
    {
        & dotnet tool install dotnet-reportgenerator-globaltool --tool-path $toolsDirectory

        if ($LASTEXITCODE -ne 0)
        {
            exit $LASTEXITCODE
        }
    }

    $coverageReportPattern = [System.IO.Path]::Combine($testProjectResultsDirectory, '*', 'coverage.cobertura.xml')

    & $reportGeneratorPath "-reports:$coverageReportPattern" "-targetdir:$testResultsDirectory"

    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }

    if ($OpenReport)
    {
        Invoke-Item (Join-Path $testResultsDirectory 'index.htm')
    }
}
finally
{
    Pop-Location
}
