$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.24/MarkdownMonsterSetup-1.24.12.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "DF6E16DBFCF96D919216AD03D70720483862F8155E9D0326B900C595DD696826" -checksumType "sha256"
