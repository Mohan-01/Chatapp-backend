$pgpKey = "A76D139724C11D61BB2CC44688D439D0E1FB7290"
$appSettingsFiles = Get-ChildItem -Path . -Filter "appsettings*.json" | Where-Object { $_.Name -notlike "*.enc.json" }

Write-Host "[SOPS] Encrypting configuration files..."

foreach ($file in $appSettingsFiles) {
    $encFile = $file.Name -replace "\.json$", ".enc.json"
    Write-Host "Encrypting $($file.Name) to $encFile"
    sops --encrypt --pgp $pgpKey $file.FullName | Out-File -Encoding UTF8 -FilePath $encFile
}

Write-Host "[SOPS] All files encrypted."
 