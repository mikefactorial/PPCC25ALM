# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# Use Powershell to create a self signed certificate for testing purposes.
Import-Module PKI

# Create a secure string for the password
$password = ConvertTo-SecureString -String "EnterPassword" -Force -AsPlainText

# Create a self-signed certificate
$certificate = New-SelfSignedCertificate -Subject "CN=example.com, O=example, C=US" -DnsName "example.com" -Type CodeSigning -KeyUsage DigitalSignature -CertStoreLocation Cert:\CurrentUser\My -FriendlyName "Plugin Self Signed Cert"

# Export the certificate to PFX
Export-PfxCertificate -Cert $certificate -FilePath "$PSScriptRoot/plugin.pfx" -Password $password