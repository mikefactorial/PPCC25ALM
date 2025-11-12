$certPath = Join-Path -Path $PSScriptRoot -ChildPath "plugin.pfx"

$pluginPackagePath = Join-Path -Path $PSScriptRoot -ChildPath  "../ServiceBus/bin/Release/ServiceBus.1.0.0.nupkg"

Write-Host "Signing the plugin package at $pluginPackagePath with the certificate at $certPath" -ForegroundColor Green
# The "--timestamper <TIMESTAMPINGSERVER>" argument should be added to timestamp the signature for production scenarios.
dotnet nuget sign $pluginPackagePath --certificate-path $certPath --certificate-password "EnterPassword" --hash-algorithm SHA256 --overwrite