param(
    [string]$WebGLBuildDir = "./Build/WebGL",
    [int]$Port = 8080
)

# Resolve absolute path
$resolvedPath = Resolve-Path -Path $WebGLBuildDir -ErrorAction SilentlyContinue
if (-not $resolvedPath) {
    Write-Error "WebGL build folder not found at '$WebGLBuildDir'"
    exit 1
}
$absPath = $resolvedPath.Path
Write-Host "Windows path: $absPath"

# Convert Windows path to WSL path
$wslPath = $(wsl wslpath -a ($absPath -replace '\\','/')).Trim()
Write-Host "WSL path: $wslPath"

# Run http-server with HTTPS, CORS, Brotli/Gzip
Write-Host "Starting http-server in WSL hosting '$absPath' on https://localhost:$Port"
wsl sh -c "cd '$wslPath' && npx http-server -p $Port -S -C cert.pem -K key.pem --brotli --cors"
