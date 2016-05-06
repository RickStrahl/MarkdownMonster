# param([string]$uid = "rstrahl", [string]$pwd = "")

Set-ExecutionPolicy Bypass -Scope CurrentUser

$uid= Read-Host -Prompt 'Username' 
$pwd=Read-Host -Prompt 'Password' -AsSecureString
$pwd = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
       [Runtime.InteropServices.Marshal]::SecureStringToBSTR($pwd))

if(!$pwd) {Exit;}

cls

curl.exe -T ".\builds\CurrentRelease\HelpBuilderSetup.exe"  "ftp://west-wind.com/Westwind_sysroot/Ftp/Files/" -u ${uid}:${pwd}
curl.exe -T ".\builds\CurrentRelease\HelpBuilderSetup.zip"  "ftp://west-wind.com/Westwind_sysroot/Ftp/Files/" -u ${uid}:${pwd}
curl.exe -T ".\builds\CurrentRelease\wwHelp500.xml"  "ftp://west-wind.com/Westwind_sysroot/Ftp/Files/" -u ${uid}:${pwd}

pause