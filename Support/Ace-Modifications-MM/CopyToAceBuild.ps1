$acePath = "\projects\ace"
$build = $false;
if ($args) 
{ 
    $build = $true
}


$curPath = "$PSScriptRoot"

# Copy Raw Code Formatting
copy mode\*.* $acePath\lib\ace\mode

# Copy Themes
copy themes\*.* $acePath\lib\ace\theme

if($build) {
    Set-Location $acePath
    npm install
    node Makefile.dryice.js

    Set-Location $curPath

    # Copy already formatted http-mode
    Copy-Item compiled-modes\mode-http.js $acePath\build\src\mode-http.js
}