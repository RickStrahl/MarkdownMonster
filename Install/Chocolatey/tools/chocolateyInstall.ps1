$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.25/MarkdownMonsterSetup-1.25.6.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "ADF0E4F7AB74E622F262E3B3B8BF25B2E3FBFC6BC89CFB8C14BAB747C7F27953" -checksumType "sha256"
