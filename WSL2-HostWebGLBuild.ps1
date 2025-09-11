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

# Convert Windows path to WSL path safely
$wslPath = $(wsl wslpath -a ($absPath -replace '\\','/')).Trim()
Write-Host "WSL path: $wslPath"

# Start Python HTTP server in WSL
Write-Host "Serving Unity WebGL build on http://localhost:$Port/"
wsl sh -c "cd '$wslPath' && python3 -m http.server $Port"
