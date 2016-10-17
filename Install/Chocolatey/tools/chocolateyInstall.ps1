$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonster/raw/master/Install/Builds/PreRelease/MarkdownMonsterSetup-0.54.exe'
$silentArgs = '/SILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "c5ab94dacd1a713d702d6c4a838d5c0473a3c424498d6410ef3a3291b2784085" -checksumType "sha256"
