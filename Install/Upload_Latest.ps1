# param([string]$uid = "uid", [string]$pwd = "")

Set-ExecutionPolicy Bypass -Scope CurrentUser

$uid= Read-Host -Prompt 'Username' 
$pwd=Read-Host -Prompt 'Password' -AsSecureString
$pwd = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pwd))

if(!$pwd) {Exit;}

curl.exe -T ".\Builds\CurrentRelease\MarkdownMonsterSetup.exe"  "ftps://west-wind.com/Westwind_sysroot/Ftp/Files/MarkdownMonsterSetup_Latest.exe" -u ${uid}:${pwd} -k
