$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.24/MarkdownMonsterSetup-1.24.12.18.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "7DACDAF7052068DC8E8848B84ACAC9A5B7C508DA8002AAB40443C14A91533B5D" -checksumType "sha256"
