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

# Run http-server with HTTPS, CORS
Write-Host "Starting http-server in WSL hosting '$absPath' on http://localhost:$Port"
wsl sh -c "cd '$wslPath' && npx http-server -p $Port -S -C cert.pem -K key.pem --cors"
