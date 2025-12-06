# Find and remove existing TopNotify appx packages

$packages = Get-AppxPackage -Name "*TopNotify*"

If ($packages) {
    $packages | Select-Object Name, PackageFullName
    $packages | Remove-AppxPackage
} Else {
    Write-Host "No existing TopNotify package found."
}

# Build the TopNotify project in Release configuration for x64 platform

dotnet build .\TopNotify.csproj -c Release -p:Platform=x64

# Install the newly built TopNotify MSIX package for testing

cmd /c '.\Install MSIX For Testing.bat'