$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.25/MarkdownMonsterSetup-1.25.14.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "406EC9C06CB1ACC55A01D627BF7254903D6A977F4054B6B5C57B0944014A6E91" -checksumType "sha256"
