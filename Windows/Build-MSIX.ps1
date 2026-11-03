param(
    [string]$Version = "1.0.0.0",
    [string]$PackageName = "RUHIApplications.RUHIBrowser",
    [string]$Publisher = "CN=A40D38DC-17E2-4A27-AA72-6B2379CD25A9",
    [string]$PublisherDisplayName = "RUHI Applications",
    [string]$DisplayName = "RUHI Browser"
)

$ErrorActionPreference = "Stop"
$rootDir = $PSScriptRoot
$publishDir = "$rootDir\bin\Publish_x64"
$stagingDir = "$rootDir\bin\MSIX_Staging"
$outputMsix = "$rootDir\bin\RuhiBrowser_${Version}_x64.msix"
$pfxPath = "$rootDir\bin\RuhiBrowser_DevCert.pfx"
$makeappx = "C:\Users\user\.nuget\packages\microsoft.windows.sdk.buildtools\10.0.22621.756\bin\10.0.22621.0\x64\makeappx.exe"
$signtool = "C:\Users\user\.nuget\packages\microsoft.windows.sdk.buildtools\10.0.22621.756\bin\10.0.22621.0\x64\signtool.exe"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host " Building MSIX Package for RUHI Browser v$Version " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# 1. Publish Release binary
Write-Host "`n[1/5] Publishing .NET Release (win-x64)..." -ForegroundColor Yellow
dotnet publish "$rootDir\RuhiBrowser.csproj" -c Release -r win-x64 --self-contained false -o $publishDir | Out-Host

# 2. Prepare Staging directory
Write-Host "`n[2/5] Preparing staging files..." -ForegroundColor Yellow
if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
New-Item -ItemType Directory -Path "$stagingDir\Assets" -Force | Out-Null
Copy-Item "$publishDir\*" $stagingDir -Recurse -Force

# 3. Generate Visual Assets
Write-Host "`n[3/5] Generating visual assets (Tiles, Logos, Splash)..." -ForegroundColor Yellow
Add-Type -AssemblyName System.Drawing
$srcIcon = "$rootDir\Assets\app_icon.png"
$src = [System.Drawing.Image]::FromFile($srcIcon)

function Resize-Img($img, $w, $h, $dest) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.DrawImage($img, 0, 0, $w, $h)
    $g.Dispose()
    $bmp.Save($dest, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}

function Create-CenteredCanvas($img, $w, $h, $size, $bgHex, $dest) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.ColorTranslator]::FromHtml($bgHex))
    $x = ($w - $size) / 2
    $y = ($h - $size) / 2
    $g.DrawImage($img, $x, $y, $size, $size)
    $g.Dispose()
    $bmp.Save($dest, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}

Resize-Img $src 44 44 "$stagingDir\Assets\Square44x44Logo.png"
Resize-Img $src 150 150 "$stagingDir\Assets\Square150x150Logo.png"
Resize-Img $src 50 50 "$stagingDir\Assets\StoreLogo.png"
Create-CenteredCanvas $src 310 150 100 "#181818" "$stagingDir\Assets\Wide310x150Logo.png"
Create-CenteredCanvas $src 620 300 140 "#181818" "$stagingDir\Assets\SplashScreen.png"
$src.Dispose()

# 4. Generate AppxManifest.xml
Write-Host "`n[4/5] Generating AppxManifest.xml..." -ForegroundColor Yellow
$manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package
  xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
  xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
  xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
  IgnorableNamespaces="uap rescap">

  <Identity
    Name="$PackageName"
    ProcessorArchitecture="x64"
    Publisher="$Publisher"
    Version="$Version" />

  <Properties>
    <DisplayName>$DisplayName</DisplayName>
    <PublisherDisplayName>$PublisherDisplayName</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
    <Description>RUHI Browser - Fast, lightweight, privacy-focused modern web browser with built-in ad blocker and AI assistant.</Description>
  </Properties>

  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.22621.0" />
  </Dependencies>

  <Resources>
    <Resource Language="en-us" />
  </Resources>

  <Applications>
    <Application Id="RuhiBrowser"
      Executable="RuhiBrowser.exe"
      EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements
        DisplayName="$DisplayName"
        Description="RUHI Browser - Fast, Lightweight, Privacy-First Web Browser"
        BackgroundColor="#181818"
        Square150x150Logo="Assets\Square150x150Logo.png"
        Square44x44Logo="Assets\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="Assets\Wide310x150Logo.png" ShortName="$DisplayName" />
        <uap:SplashScreen Image="Assets\SplashScreen.png" BackgroundColor="#181818" />
      </uap:VisualElements>
      <Extensions>
        <uap:Extension Category="windows.protocol">
          <uap:Protocol Name="http">
            <uap:DisplayName>$DisplayName HTTP</uap:DisplayName>
          </uap:Protocol>
        </uap:Extension>
        <uap:Extension Category="windows.protocol">
          <uap:Protocol Name="https">
            <uap:DisplayName>$DisplayName HTTPS</uap:DisplayName>
          </uap:Protocol>
        </uap:Extension>
        <uap:Extension Category="windows.fileTypeAssociation">
          <uap:FileTypeAssociation Name="html">
            <uap:SupportedFileTypes>
              <uap:FileType>.html</uap:FileType>
              <uap:FileType>.htm</uap:FileType>
              <uap:FileType>.pdf</uap:FileType>
            </uap:SupportedFileTypes>
          </uap:FileTypeAssociation>
        </uap:Extension>
      </Extensions>
    </Application>
  </Applications>

  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
  </Capabilities>
</Package>
"@
[System.IO.File]::WriteAllText("$stagingDir\AppxManifest.xml", $manifest, [System.Text.Encoding]::UTF8)

# 5. Pack and Sign
Write-Host "`n[5/5] Packing and Signing MSIX..." -ForegroundColor Yellow
if (Test-Path $outputMsix) { Remove-Item $outputMsix -Force }
& $makeappx pack /d $stagingDir /p $outputMsix /o | Out-Host

# Ensure matching certificate exists for $Publisher
$needNewCert = $true
if (Test-Path $pfxPath) {
    try {
        $existingCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($pfxPath, "Ruhi12345")
        if ($existingCert.Subject -eq $Publisher) {
            $needNewCert = $false
        }
    } catch { }
}

if ($needNewCert) {
    Write-Host "Creating matching developer certificate for $Publisher..." -ForegroundColor Cyan
    $cert = New-SelfSignedCertificate -Type Custom -Subject $Publisher -KeyUsage DigitalSignature -FriendlyName "RUHI Browser Store Dev Certificate" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3")
    $pwd = ConvertTo-SecureString -String "Ruhi12345" -Force -AsPlainText
    Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $pwd | Out-Null
    $cerPath = "$rootDir\bin\RuhiBrowser_DevCert.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
}

if (Test-Path $pfxPath) {
    & $signtool sign /fd SHA256 /a /f $pfxPath /p "Ruhi12345" $outputMsix | Out-Host
}

Write-Host "`n=============================================" -ForegroundColor Green
Write-Host " MSIX PACKAGE CREATED SUCCESSFULLY! " -ForegroundColor Green
Write-Host " File: $outputMsix " -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
