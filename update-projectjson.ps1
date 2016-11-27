# This script sets the project.json version to to be the version of the appveyor build. This is so our nupkg's will have the appveyor build number in them

$projectJsonFileLocation = "src/StatsdClient/project.json"
$newVersion = $env:APPVEYOR_BUILD_VERSION
if($newVersion -eq $null)
{
  return
}

Write-Host "$projectJsonFileLocation will be update with new version '$newVersion'"

$json = (Get-Content $projectJsonFileLocation -Raw) | ConvertFrom-Json
$json.version = $newVersion
$json | ConvertTo-Json -depth 100 | Out-File $projectJsonFileLocation