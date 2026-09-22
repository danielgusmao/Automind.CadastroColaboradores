$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$dist = Join-Path $root "dist"

if (Test-Path $dist) {
    Remove-Item $dist -Recurse -Force
}

New-Item -ItemType Directory -Path $dist -Force | Out-Null

$packages = @(
    @{ Name = "Brave";  Folder = "Brave"  },
    @{ Name = "Chrome"; Folder = "Chrome" }
)

foreach ($package in $packages) {
    $source = Join-Path $root $package.Folder
    $zip = Join-Path $dist ("AutomindTopdeskBridge-{0}-v1.0.0.zip" -f $package.Name)

    if (-not (Test-Path $source)) {
        throw "Pasta nao encontrada: $source"
    }

    Compress-Archive -Path (Join-Path $source "*") -DestinationPath $zip -Force
    Write-Host "Gerado: $zip"
}

Write-Host "Pacotes gerados com sucesso em: $dist"
