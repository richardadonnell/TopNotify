# Find and remove existing TopNotify appx packages

Get-AppxPackage -Name "*TopNotify*" | Select-Object Name, PackageFullName

If ($?) {
    Get-AppxPackage -Name "*TopNotify*" | Remove-AppxPackage
} Else {
    Write-Host "No existing TopNotify package found."
}

# Build the TopNotify project in Release configuration for x64 platform

dotnet build .\TopNotify.csproj -c Release -p:Platform=x64

# Install the newly built TopNotify MSIX package for testing

cmd /c '.\Install MSIX For Testing.bat'