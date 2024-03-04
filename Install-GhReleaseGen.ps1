[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Configuration = "Release"
)

$osName = $null

if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
    $osName = "osx"
}
elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
    $osName = "linux"
}
elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    $osName = "win"
}
else {
    $PSCmdlet.ThrowTerminatingError(
        [System.Management.Automation.ErrorRecord]::new(
            [System.PlatformNotSupportedException]::new("Unsupported OS platform."),
            "UnsupportedOS",
            [System.Management.Automation.ErrorCategory]::NotImplemented,
            $null
        )
    )
}

$osArch = $null
if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) {
    $osArch = "arm64"
}
elseif ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq [System.Runtime.InteropServices.Architecture]::X64) {
    $osArch = "x64"
}
else {
    $PSCmdlet.ThrowTerminatingError(
        [System.Management.Automation.ErrorRecord]::new(
            [System.PlatformNotSupportedException]::new("Unsupported OS architecture."),
            "UnsupportedArch",
            [System.Management.Automation.ErrorCategory]::NotImplemented,
            $null
        )
    )
}

$artifactPath = Join-Path -Path $PSScriptRoot -ChildPath "artifacts/publish/ConsoleApp/$($Configuration.ToLower())_$($osName)-$($osArch)/gh-releasegen"
$dsymArtifactPath = Join-Path -Path $PSScriptRoot -ChildPath "artifacts/publish/ConsoleApp/$($Configuration.ToLower())_$($osName)-$($osArch)/gh-releasegen.dsym"
$pdbArtifactPath = Join-Path -Path $PSScriptRoot -ChildPath "artifacts/publish/ConsoleApp/$($Configuration.ToLower())_$($osName)-$($osArch)/gh-releasegen.pdb"
$installPath = Join-Path -Path ([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::UserProfile)) -ChildPath ".ghreleasegen/bin/"

$dsymInstallPath = Join-Path -Path $installPath -ChildPath "gh-releasegen.dsym"
$pdbInstallPath = Join-Path -Path $installPath -ChildPath "gh-releasegen.pdb"

if (!(Test-Path -Path $artifactPath)) {
    $PSCmdlet.ThrowTerminatingError(
        [System.Management.Automation.ErrorRecord]::new(
            [System.IO.FileNotFoundException]::new("Built executable not found at '$($artifactPath)'."),
            "InstallerNotFound",
            [System.Management.Automation.ErrorCategory]::ObjectNotFound,
            $artifactPath
        )
    )
}

if (!(Test-Path -Path $installPath)) {
    Write-Warning "Creating install directory at: $($installPath)"
    $null = New-Item -Path $installPath -ItemType "Directory" -Force
}

Write-Verbose "Copying 'gh-releasegen' to: $($installPath)"
Copy-Item -Path $artifactPath -Destination $installPath -Force -Verbose:$false

if ($Configuration -ne "Release") {
    if (Test-Path -Path $dsymArtifactPath) {
        Write-Verbose "Copying 'gh-releasegen.dsym' to: $($installPath)"
        Copy-Item -Path $dsymArtifactPath -Destination $installPath -Force -Recurse -Verbose:$false
    }

    if (Test-Path -Path $pdbArtifactPath) {
        Write-Verbose "Copying 'gh-releasegen.pdb' to: $($installPath)"
        Copy-Item -Path $pdbArtifactPath -Destination $installPath -Force -Verbose:$false
    }
}
else {
    if (Test-Path -Path $dsymInstallPath) {
        Write-Verbose "Removing 'gh-releasegen.dsym' from: $($installPath)"
        Remove-Item -Path $dsymInstallPath -Force -Recurse -Verbose:$false
    }

    if (Test-Path -Path $pdbInstallPath) {
        Write-Verbose "Removing 'gh-releasegen.pdb' from: $($installPath)"
        Remove-Item -Path $pdbInstallPath -Force -Verbose:$false
    }
}
