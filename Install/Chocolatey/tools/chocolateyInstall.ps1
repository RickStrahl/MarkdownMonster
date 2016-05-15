$packageName = 'MarkdownMonster'
$fileType = 'exe'
$url = 'http://west-wind.com/files/MarkdownMonsterSetup.exe'
$silentArgs = '/q'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes $validExitCodes
