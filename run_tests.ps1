# Runs every test in tests\inputs against one version and compares with tests\expected.
# Usage (Windows PowerShell):   .\run_tests.ps1 version-B-polymorphic
#                               .\run_tests.ps1 version-A-flat
# Needs the .NET 8 SDK. Clock times in the output are replaced with <TIME> before comparing.
param([string]$Version = "version-B-polymorphic")
$Root = $PSScriptRoot
Write-Host "Building $Version ..."
dotnet build "$Root\$Version" -nologo -v q -o "$Root\.build\$Version" | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed"; exit 1 }
$Work = Join-Path $env:TEMP ("pmtests_" + [guid]::NewGuid())
New-Item -ItemType Directory $Work | Out-Null
Push-Location $Work
$script:pass = 0; $script:fail = 0
function Run-Test($name) {
    $out = Get-Content "$Root\tests\inputs\$name.txt" | dotnet "$Root\.build\$Version\PatientManagement.dll" 2>&1
    $out = @($out | ForEach-Object { ($_ -replace '\d{2}/\d{2} \d{2}:\d{2}:\d{2}', '<TIME>') -replace 'arrived \d{2}:\d{2}:\d{2}', 'arrived <TIME>' })
    $expected = @(Get-Content "$Root\tests\expected\$name.txt")
    if (($out -join "`n").TrimEnd() -eq ($expected -join "`n").TrimEnd()) { Write-Host "PASS  $name"; $script:pass++ }
    else { Write-Host "FAIL  $name"; $script:fail++ }
}
foreach ($f in Get-ChildItem "$Root\tests\inputs\*.txt") {
    $t = $f.BaseName
    if ($t -eq "T5b_reload") { continue }
    Remove-Item patientmanagement.txt -ErrorAction SilentlyContinue
    Run-Test $t
    if ($t -eq "T5a_save") { Run-Test "T5b_reload" }
}
Pop-Location
Remove-Item $Work -Recurse -Force
Write-Host "$($script:pass) passed, $($script:fail) failed"
