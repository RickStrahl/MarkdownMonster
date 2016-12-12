# Script to fix  duplicatated path values for Markdown Monster
# IMPORTANT: Has to run as an administrator

# Get it
$path = [System.Environment]::GetEnvironmentVariable(
    'PATH',
    'Machine'
)

"old path:"
"$path"

# Remove unwanted elements
$path = ($path.Split(';') | Get-Unique) -join ';'

""
"updated path:"
"$path"

# Set it - will fail when not an administrator
[System.Environment]::SetEnvironmentVariable(
   'PATH',
   $path,
   'Machine'
)
