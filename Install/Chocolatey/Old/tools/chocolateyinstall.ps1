$packageName= 'markdownmonster' 

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$fileLocation = Join-Path $toolsDir 'MarkdownMonsterSetup.exe'
$fileType = 'exe'
$silentArgs = '/q2'

Install-ChocolateyInstallPackage $packageName $fileType $silentArgs $fileLocation