param(
    [string]$WebGLBuildDir = "./Build/WebGL",
    [string]$ServerDataDir = "./ServerData",
    [string]$LanIP = "",
    [string]$MkcertExe = ""
)
Push-Location $PSScriptRoot

function Ensure-Elevated {
    if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
        ).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) {

        $scriptPath = $MyInvocation.MyCommand.Path
        if (-not $scriptPath) { $scriptPath = $PSCommandPath }
        $scriptDir = Split-Path -Parent $scriptPath
        $scriptArgs = $MyInvocation.MyCommand.UnboundArguments

        $argList = @(
            "-NoExit"
            "-ExecutionPolicy"
            "Bypass"
            "-File"
            "`"$scriptPath`""
        )
        if ($scriptArgs) {
            $argList += $scriptArgs
        }

        Write-Host ">>> Start-Process powershell.exe -Verb RunAs -WorkingDirectory `"$scriptDir`" -ArgumentList ($argList -join ',')"

        Start-Process -FilePath "powershell.exe" `
            -Verb RunAs `
            -WorkingDirectory $scriptDir `
            -ArgumentList $argList
        Exit
    }
}

function Get-MkcertCerts {
    $results = @()
    foreach ($loc in "LocalMachine", "CurrentUser") {
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", $loc)
        try {
            $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)
            $results += $store.Certificates | Where-Object {
                $_.Subject -match "(?i)mkcert" -or $_.Issuer -match "(?i)mkcert"
            } | ForEach-Object {
                [PSCustomObject]@{ Subject = $_.Subject; Expires = $_.NotAfter; Store = $loc }
            }
        }
        finally {
            $store.Close()
        }
    }
    return $results
}

function Ensure-Directory($targetDir, $name) {
    $fullPath = Resolve-Path -Path $targetDir -ErrorAction SilentlyContinue
    if (-not $fullPath) {
        Write-Host "[*] Directory ($name) not found at '$targetDir'. Creating it..."
        New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
        $fullPath = Resolve-Path -Path $targetDir
    }
    return $fullPath.Path
}

# --- Ensure Directories ---
$absWebGLDir = Ensure-Directory $WebGLBuildDir "WebGLBuildDir"
$absServerDataDir = Ensure-Directory $ServerDataDir "ServerDataDir"

Write-Host "Saving certificates to: "
Write-Host " - WebGLBuildDir: $absWebGLDir"
Write-Host " - ServerDataDir: $absServerDataDir"

# --- Locate mkcert executable ---
if (-not [string]::IsNullOrWhiteSpace($MkcertExe)) {
    if (-not (Test-Path $MkcertExe)) {
        Write-Error "Provided mkcert executable not found at: $MkcertExe"
        exit 1
    }
    $mkcertPath = (Resolve-Path $MkcertExe).Path
}
else {
    $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
    $candidate = Get-ChildItem -Path $scriptRoot -Filter "mkcert*.exe" -File -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($candidate) {
        $mkcertPath = $candidate.FullName
    }
    elseif (Get-Command mkcert -ErrorAction SilentlyContinue) {
        $mkcertPath = "mkcert"
    }
    else {
        Write-Error "mkcert not found. Download from: https://github.com/FiloSottile/mkcert"
        exit 1
    }
}
Write-Host "Using mkcert at: $mkcertPath"

# --- Ensure mkcert root CA is installed ---
Write-Host ""
Write-Host "============================================="
Write-Host " Checking mkcert root CA installation "
Write-Host "---------------------------------------------"

$mkcerts = Get-MkcertCerts
if ($mkcerts.Count -gt 0) {
    foreach ($c in $mkcerts) {
        Write-Host "  Found: $($c.Subject) (Expires $($c.Expires.ToShortDateString())) in $($c.Store)"
    }
    Write-Host "[OK] mkcert root CA already trusted by Windows store."
}
else {
    Ensure-Elevated
    Write-Host "[*] Running: $mkcertPath -install"
    & $mkcertPath -install
    $mkcerts = Get-MkcertCerts
    if ($mkcerts.Count -eq 0) {
        Write-Error "[X] mkcert root CA not found even after install. Run manually."
        exit 1
    }
}
Write-Host "---------------------------------------------"
Write-Host " End of mkcert root CA check"
Write-Host "============================================="

# --- Detect IPv4 addresses ---
$ips = Get-NetIPAddress -AddressFamily IPv4

if ([string]::IsNullOrWhiteSpace($LanIP)) {
    $preferred = $ips | Where-Object { $_.InterfaceAlias -match "^Ethernet$" }
    if (-not $preferred) {
        $preferred = $ips | Select-Object -First 1
    }
    $LanIP = $preferred.IPAddress
    Write-Host "Auto-detected LAN IP: $LanIP"
}
else {
    Write-Host "Using provided LAN IP: $LanIP"
}

Write-Host ""
Write-Host "=== Detected IPv4 addresses ==="
foreach ($ip in $ips) {
    if ($ip.IPAddress -eq $LanIP) {
        Write-Host ("  * {0} ({1})" -f $ip.IPAddress, $ip.InterfaceAlias) -ForegroundColor Green
    }
    else {
        Write-Host ("    {0} ({1})" -f $ip.IPAddress, $ip.InterfaceAlias)
    }
}
Write-Host "================================"

# --- Generate certificates in both dirs ---
foreach ($dir in @($absWebGLDir, $absServerDataDir)) {
    Push-Location $dir
    Write-Host ""
    Write-Host "============================================="
    Write-Host " Generating certificate in $dir"
    Write-Host "---------------------------------------------"
    Write-Host "Running: $mkcertPath -cert-file cert.pem -key-file key.pem localhost 127.0.0.1 $LanIP"
    Write-Host "---------------------------------------------"
    & $mkcertPath -cert-file cert.pem -key-file key.pem localhost 127.0.0.1 $LanIP
    Write-Host "---------------------------------------------"
    Pop-Location

    Write-Host "Certificate created: $dir\cert.pem and key.pem"
}

Write-Host "============================================="
Write-Host "   You can now use:"
Write-Host "     https://localhost:<port>/"
Write-Host "     https://${LanIP}:<port>/"
Write-Host "============================================="
Read-Host
