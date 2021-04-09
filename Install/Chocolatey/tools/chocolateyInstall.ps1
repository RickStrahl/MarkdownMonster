$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.26/MarkdownMonsterSetup-1.26.10.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "40187FDF08648CC19CB628F46214EC49688CA559F9521152EB20A295AB7C1CBC" -checksumType "sha256"
