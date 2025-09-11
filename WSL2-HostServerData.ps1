param(
    [string]$ServerDataDir = "./ServerData",
    [int]$Port = 2505
)

# Resolve absolute path
$resolvedPath = Resolve-Path -Path $ServerDataDir -ErrorAction SilentlyContinue
if (-not $resolvedPath) {
    Write-Error "Target directory does not exist at '$ServerDataDir'"
    exit 1
}
$absPath = $resolvedPath.Path
Write-Host "Windows path: '$absPath'"

# Convert Windows path to WSL path
$wslPath = $(wsl wslpath -a ($absPath -replace '\\','/')).Trim()
Write-Host "WSL path: '$wslPath'"

# Create a temporary Python file on Windows
$tempPyFile = [System.IO.Path]::Combine($env:TEMP, "hostserverdata_corsserver.py")

# Python script content
$pyScriptContent = @"
from http.server import HTTPServer, SimpleHTTPRequestHandler
import os

class CORSRequestHandler(SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        super().end_headers()

os.chdir(r'$wslPath')
server_address = ('0.0.0.0', $Port)
httpd = HTTPServer(server_address, CORSRequestHandler)
print(f"Serving '$wslPath' with CORS on 'http://localhost:$Port'")
httpd.serve_forever()
"@

# Write content to the temp file
Set-Content -Path $tempPyFile -Value $pyScriptContent -Encoding UTF8

# Convert temp file path to WSL path
$wslPyFile = $(wsl wslpath -a ($tempPyFile -replace '\\','/')).Trim()

# Start the Python server in WSL
Write-Host "Starting Python CORS server in WSL hosting '$absPath' on 'http://localhost:$Port'"
wsl sh -c "python3 '$wslPyFile'"