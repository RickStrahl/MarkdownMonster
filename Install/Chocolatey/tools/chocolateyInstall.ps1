$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonster/raw/master/Install/Builds/PreRelease/MarkdownMonsterSetup-0.52.exe'
$silentArgs = '/SILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "422DE6578B0D8272913D327A0F08D1EC6C970D62E5B2C4F10D4CEFF255B53461" -checksumType "sha256"
