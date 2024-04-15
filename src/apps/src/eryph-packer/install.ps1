<#
    .SYNOPSIS
    Downloads and installs eryph-packer on the local machine.

    .DESCRIPTION
    Retrieves the eryph-packer package for the latest or a specified version,
    and downloads and installs the application to the local machine.

    .NOTES
    =====================================================================

    Copyright 2022 dbosoft GmbH,

    Based on Chocolatey installation script, original copyright:

    Copyright 2017 - 2020 Chocolatey Software, Inc, and the
    original authors/contributors from ChocolateyGallery
    Copyright 2011 - 2017 RealDimensions Software, LLC, and the
    original authors/contributors from ChocolateyGallery
    at https://github.com/chocolatey/chocolatey.org

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
    =====================================================================

    Environment Variables, specified as $env:NAME in PowerShell.exe and %NAME% in cmd.exe.
    For explicit proxy, please set $env:eryphProxyLocation and optionally $env:eryphProxyUser and $env:eryphProxyPassword
    For an explicit version of eryph, please set $env:eryphVersion = 'versionnumber'
    To target a different url for eryph.zip, please set $env:eryphDownloadUrl = 'full url to zip file'
    NOTE: $env:eryphDownloadUrl does not work with $env:eryphVersion.
    To bypass the use of any proxy, please set $env:eryphIgnoreProxy = 'true'


#>
[CmdletBinding(DefaultParameterSetName = 'Default')]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingConvertToSecureStringWithPlainText", "")]

param(
    # The URL to download eryph from. This defaults to the value of
    # $env:eryphDownloadUrl, if it is set, and otherwise falls back to the
    # official eryph community repository to download the eryph package.
    # Can be used for offline installation by providing a path to a eryph.zip
    [Parameter(Mandatory = $false)]
    [string]
    $DownloadUrl = $env:eryphDownloadUrl,

    # Specifies a target version of eryph to install. By default, the latest
    # stable version is installed. This will use the value in
    # $env:eryphVersion by default, if that environment variable is present.
    # This parameter is ignored if -eryphDownloadUrl is set.
    [Parameter(Mandatory = $false)]
    [string]
    $Version = $env:eryphVersion,

    # The path to installer eryph-packer. This defaults to 
    # $env:LOCALAPPDATA\eryph\packer
    [Parameter(Mandatory = $false)]
    [string]
    $InstallPath = "${env:LOCALAPPDATA}\eryph\packer",

    
    # If set, ignores any configured proxy. This will override any proxy
    # environment variables or parameters. This will be set by default if
    # $env:eryphIgnoreProxy is set to a value other than 'false' or '0'.
    [Parameter(Mandatory = $false)]
    [switch]
    $IgnoreProxy = $(
        $envVar = "$env:eryphIgnoreProxy".Trim()
        $value = $null
        if ([bool]::TryParse($envVar, [ref] $value)) {
            $value
        }
        elseif ([int]::TryParse($envVar, [ref] $value)) {
            [bool]$value
        }
        else {
            [bool]$envVar
        }
    ),

    # Specifies the proxy URL to use during the download. This will default to
    # the value of $env:eryphProxyLocation, if any is set.
    [Parameter(ParameterSetName = 'Proxy', Mandatory = $false)]
    [string]
    $ProxyUrl = $env:eryphProxyLocation,

    # Specifies the credential to use for an authenticated proxy. By default, a
    # proxy credential will be constructed from the $env:eryphProxyUser and
    # $env:eryphProxyPassword environment variables, if both are set.
    [Parameter(ParameterSetName = 'Proxy', Mandatory = $false)]
    [System.Management.Automation.PSCredential]
    $ProxyCredential,

    [Parameter(Mandatory = $false)]
    [switch]
    $Force,

    # Download the latest version even it is unstable
    [Parameter(Mandatory = $false)]
    [switch]
    $Unstable,

    # EMail address for invitation code download
    [Parameter(Mandatory = $false)]
    [string]
    $EMail,

    # Code for invitation code download
    [Parameter(Mandatory = $false)]
    [string]
    $InvitationCode
)

#region Functions

Function Test-CommandExist {
[CmdletBinding()]
    [OutputType([System.Boolean])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $command
    )

    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = "stop"

    try {
        if(Get-Command $command){
            $true
        }
    } catch {
        $false
    } finally {
        $ErrorActionPreference=$oldPreference
    }

}


function Get-Downloader {
    <#
    .SYNOPSIS
    Gets a System.Net.WebClient that respects relevant proxies to be used for
    downloading data.

    .DESCRIPTION
    Retrieves a WebClient object that is pre-configured according to specified
    environment variables for any proxy and authentication for the proxy.
    Proxy information may be omitted if the target URL is considered to be
    bypassed by the proxy (originates from the local network.)

    .PARAMETER Url
    Target URL that the WebClient will be querying. This URL is not queried by
    the function, it is only a reference to determine if a proxy is needed.

    .EXAMPLE
    Get-Downloader -Url $fileUrl

    Verifies whether any proxy configuration is needed, and/or whether $fileUrl
    is a URL that would need to bypass the proxy, and then outputs the
    already-configured WebClient object.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]
        $Url,

        [Parameter(Mandatory = $false)]
        [string]
        $ProxyUrl,

        [Parameter(Mandatory = $false)]
        [System.Management.Automation.PSCredential]
        $ProxyCredential
    )

    $downloader = New-Object System.Net.WebClient

    $defaultCreds = [System.Net.CredentialCache]::DefaultCredentials
    if ($defaultCreds) {
        $downloader.Credentials = $defaultCreds
    }

    if ($ProxyUrl) {
        # Use explicitly set proxy.
        Write-Verbose "Using explicit proxy server '$ProxyUrl'."
        $proxy = New-Object System.Net.WebProxy -ArgumentList $ProxyUrl, <# bypassOnLocal: #> $true

        $proxy.Credentials = if ($ProxyCredential) {
            $ProxyCredential.GetNetworkCredential()
        } elseif ($defaultCreds) {
            $defaultCreds
        } else {
            Write-Warning "Default credentials were null, and no explicitly set proxy credentials were found. Attempting backup method."
            (Get-Credential).GetNetworkCredential()
        }

        if (-not $proxy.IsBypassed($Url)) {
            $downloader.Proxy = $proxy
        }
    }

    $downloader
}

function Request-String {
    <#
    .SYNOPSIS
    Downloads content from a remote server as a string.

    .DESCRIPTION
    Downloads target string content from a URL and outputs the resulting string.
    Any existing proxy that may be in use will be utilised.

    .PARAMETER Url
    URL to download string data from.

    .PARAMETER ProxyConfiguration
    A hashtable containing proxy parameters (ProxyUrl and ProxyCredential)

    .EXAMPLE
    Request-String https://community.eryph.org/install.ps1

    Retrieves the contents of the string data at the targeted URL and outputs
    it to the pipeline.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]
        $Url,

        [Parameter(Mandatory = $false)]
        [hashtable]
        $ProxyConfiguration
    )

    (Get-Downloader $url @ProxyConfiguration).DownloadString($url)
}

function Request-File {
    <#
    .SYNOPSIS
    Downloads a file from a given URL.

    .DESCRIPTION
    Downloads a target file from a URL to the specified local path.
    Any existing proxy that may be in use will be utilised.

    .PARAMETER Url
    URL of the file to download from the remote host.

    .PARAMETER File
    Local path for the file to be downloaded to.

    .PARAMETER ProxyConfiguration
    A hashtable containing proxy parameters (ProxyUrl and ProxyCredential)

    .EXAMPLE
    Request-File -Url https://community.eryph.org/install.ps1 -File $targetFile

    Downloads the install.ps1 script to the path specified in $targetFile.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]
        $Url,

        [Parameter(Mandatory = $false)]
        [string]
        $File,

        [Parameter(Mandatory = $false)]
        [hashtable]
        $ProxyConfiguration
    )

    Write-Verbose "Downloading $url to $file"
    (Get-Downloader $url @ProxyConfiguration).DownloadFile($url, $file)
}

function Enable-PSConsoleWriter {
    <#
    .SYNOPSIS
    Workaround for a bug in output stream handling PS v2 or v3.

    .DESCRIPTION
    PowerShell v2/3 caches the output stream. Then it throws errors due to the
    FileStream not being what is expected. Fixes "The OS handle's position is
    not what FileStream expected. Do not use a handle simultaneously in one
    FileStream and in Win32 code or another FileStream." error.

    .EXAMPLE
    Enable-PSConsoleWriter

    .NOTES
    General notes
    #>

    [CmdletBinding()]
    param()
    if ($PSVersionTable.PSVersion.Major -gt 3) {
        return
    }

    try {
        # http://www.leeholmes.com/blog/2008/07/30/workaround-the-os-handles-position-is-not-what-filestream-expected/ plus comments
        $bindingFlags = [Reflection.BindingFlags] "Instance,NonPublic,GetField"
        $objectRef = $host.GetType().GetField("externalHostRef", $bindingFlags).GetValue($host)

        $bindingFlags = [Reflection.BindingFlags] "Instance,NonPublic,GetProperty"
        $consoleHost = $objectRef.GetType().GetProperty("Value", $bindingFlags).GetValue($objectRef, @())
        [void] $consoleHost.GetType().GetProperty("IsStandardOutputRedirected", $bindingFlags).GetValue($consoleHost, @())

        $bindingFlags = [Reflection.BindingFlags] "Instance,NonPublic,GetField"
        $field = $consoleHost.GetType().GetField("standardOutputWriter", $bindingFlags)
        $field.SetValue($consoleHost, [Console]::Out)

        [void] $consoleHost.GetType().GetProperty("IsStandardErrorRedirected", $bindingFlags).GetValue($consoleHost, @())
        $field2 = $consoleHost.GetType().GetField("standardErrorWriter", $bindingFlags)
        $field2.SetValue($consoleHost, [Console]::Error)
    } catch {
        Write-Warning "Unable to apply redirection fix."
    }
}

function Test-EryphInstalled {
    [CmdletBinding()]
    param()

    if ($Command = Get-Command eryph-packer -CommandType Application -ErrorAction Ignore) {
        # eryph-packer is on the PATH, assume it's installed
        Write-Warning "'eryph-packer' was found at '$($Command.Path)'."
        $true
    }
}

function Get-BetaDownloadUrl {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [System.Management.Automation.PSObject]
        $productFile
    )

    Write-Information "This version can only be downloaded with an invitation code." -InformationAction Continue

    if(-not $EMail){
        $EMail = Read-Host -Prompt "Please enter your email address"
    }
    if(-not $InvitationCode){
        $InvitationCode = Read-Host -Prompt "Please enter your invitation code"
    }

    Write-Information "Requesting download url. This could take a moment..." -InformationAction Continue

    $request = @{
        beta = $productFile.Beta
        betaPath = $productFile.BetaPath
        email = $EMail
        invitationCode = $InvitationCode
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri 'https://identity-backend-eu1.dbosoft.eu/api/request/BetaDownload' -Method POST -Body $request -ContentType 'application/json' -SkipHttpErrorCheck

    if($response.message){

        $errorMessage = "Failed to retrieve download url. Error: ${response.message}"
        if($response.message -eq "invalid invitation code"){
            $errorMessage = @(
                'The invitation code is invalid.'
                'Please note that the invitation code has to match the email address.'
                'If you have not received an invitation code, please join the waitlist on https://eryph.io'
                ' '
            ) -join [Environment]::NewLine
        }
        Write-Error $errorMessage
        return
    }

    $betaUrl = $response.value.downloadUri

    if(-not $betaUrl){
        Write-Error "No download url found in response"
        return
    }
    $betaUrl

}

#endregion Functions

#region Pre-check

# Ensure we have all our streams setup correctly, needed for older PSVersions.
Enable-PSConsoleWriter

if (Test-EryphInstalled) {

    if(-not $Force)
    {
    $message = @(
        "An existing eryph-packer installation was detected. Installation will not continue."
        "For security reasons, this script will not overwrite existing installations."
        ""
    ) -join [Environment]::NewLine } else{
    $message = @(
        "An existing eryph-packer installation was detected. Installation will continue"
        "due to -Force argument. The existing installation will be overwritten."
        ""
    ) -join [Environment]::NewLine
    }

    Write-Warning $message

    if(-not $Force){
        return
    }
}

#endregion Pre-check

#region Setup

$proxyConfig = if ($IgnoreProxy -or -not $ProxyUrl) {
    @{}
} else {
    $config = @{
        ProxyUrl = $ProxyUrl
    }

    if ($ProxyCredential) {
        $config['ProxyCredential'] = $ProxyCredential
    } elseif ($env:eryphProxyUser -and $env:eryphProxyPassword) {
        $securePass = ConvertTo-SecureString $env:eryphProxyPassword -AsPlainText -Force
        $config['ProxyCredential'] = [System.Management.Automation.PSCredential]::new($env:eryphProxyUser, $securePass)
    }

    $config
}

# Attempt to set highest encryption available for SecurityProtocol.
# PowerShell will not set this by default (until maybe .NET 4.6.x). This
# will typically produce a message for PowerShell v2 (just an info
# message though)
try {
    # Set TLS 1.2 (3072) as that is the minimum required by eryph.org.
    # Use integers because the enumeration value for TLS 1.2 won't exist
    # in .NET 4.0, even though they are addressable if .NET 4.5+ is
    # installed (.NET 4.5 is an in-place upgrade).
    Write-Verbose "Forcing web requests to allow TLS v1.2 (Required for requests to releases.dbosoft.eu)"
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
}
catch {
    $errorMessage = @(
        'Unable to set PowerShell to use TLS 1.2. This is required for downloading eryph.'
        'If you see underlying connection closed or trust errors, you may need to do one or more of the following:'
        '(1) upgrade to .NET Framework 4.5+ and PowerShell v3+,'
        '(2) Call [System.Net.ServicePointManager]::SecurityProtocol = 3072; in PowerShell prior to attempting installation,'
        '(3) specify internal eryph package location (set $env:eryphDownloadUrl prior to install or host the package internally),'
        '(4) use the Download + PowerShell method of install.'
    ) -join [Environment]::NewLine
    Write-Error $errorMessage
}

if ($DownloadUrl) {
    if ($Version) {
        Write-Warning "Ignoring -Version parameter ($Version) because -DownloadUrl is set."
    }

    Write-Information "Downloading eryph-packer from: $DownloadUrl" -InformationAction Continue
} elseif ($Version) {
    Write-Information "Downloading specific version of eryph-packer: $Version" -InformationAction Continue
} else {
    Write-Information "Fetching versions of eryph-packer..." -InformationAction Continue
}

if(-not $DownloadUrl){
    $productJsonUrl = 'https://releases.dbosoft.eu/eryph/packer/index.json'
    $productJson = Request-String -Url $productJsonUrl -ProxyConfiguration $proxyConfig | ConvertFrom-Json

    if(-not $productJson){
        return
    }

    $latestVersion = $productJson.latestVersion
    Write-Verbose "Latest version of eryph-packer: ${latestVersion}"

    $stableVersion = $productJson.stableVersion
    Write-Verbose "Stable version of eryph-packer: ${stableVersion}"

    if(-not $Version){
        $Version = $latestVersion

        if(-not $Unstable){
            $Version = $stableVersion
            if(-not $Version){
                $Version = $latestVersion
            }
        }

        Write-Information "Selected version of eryph-packer: ${Version}" -InformationAction Continue
    }

    $productFile = $productJson.versions."$Version".files | Where-Object arch -eq 'amd64' | Select-Object -First 1

    if(!$productFile){
        Write-Error "Version {$Version} not found"
        return
    }

    if(-not $productFile.Beta){
        $DownloadUrl = $productFile.url
    }
    else{
        $DownloadUrl = Get-BetaDownloadUrl $productFile
        if(-not $DownloadUrl){
            return
        }
    }

}

if (-not $env:TEMP) {
    $env:TEMP = Join-Path $env:SystemDrive -ChildPath 'temp'
}

$eryphTempDir = Join-Path $env:TEMP -ChildPath "eryph"
$tempDir = Join-Path $eryphTempDir -ChildPath "packerInstall"

if (-not (Test-Path $tempDir -PathType Container)) {
    $null = New-Item -Path $tempDir -ItemType Directory
}

#endregion Setup

#region Download & Extract eryph

# If we are passed a valid local path, we do not need to download it.
if (Test-Path $DownloadUrl) {
    $file = $DownloadUrl
    
    Write-Information "Using eryph from $DownloadUrl." -InformationAction Continue
} else {
    $file = Join-Path $tempDir "eryph.zip"
    $deleteFile = $true
    Write-Information "Getting eryph-packer from $DownloadUrl." -InformationAction Continue
    Request-File -Url $DownloadUrl -File $file -ProxyConfiguration $proxyConfig
}


if((Test-CommandExist "Get-FileHash") -and $productFile) {
    Write-Verbose "Validating checksum of downloaded file."
    $expectedChecksum =  $productFile.sha256Checksum
    $actualChecksum = (Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant()

    if($expectedChecksum -ne $actualChecksum){
        Write-Error "Checksum of downloaded file doesn't match expected hash."
        return
    }
}

Write-Verbose "Extracting $file to $InstallPath"
if ($PSVersionTable.PSVersion.Major -lt 5) {
    # Determine unzipping method
    Write-Verbose 'Using built-in compression to unzip'

    try {
        $shellApplication = New-Object -ComObject Shell.Application
        $zipPackage = $shellApplication.NameSpace($file)
        $destinationFolder = $shellApplication.NameSpace($InstallPath)
        $destinationFolder.CopyHere($zipPackage.Items(), 0x10)
    } catch {
        Write-Warning "Unable to unzip package using built-in compression. "
        throw $_
    }
} else {
    Microsoft.PowerShell.Archive\Expand-Archive -Path $file -DestinationPath $InstallPath -Force
}

if($deleteFile) {
    Remove-Item $file
}
#endregion Download & Extract eryph

Write-Host "eryph-packer was installed in path $InstallPath"

Write-Host 'Ensuring eryph-packer command is on the path for current user'

$exePath = Join-Path $InstallPath -ChildPath 'bin'

# Update current process PATH environment variable if it needs updating.
if ($env:Path -notlike "*$exePath*") {
    [Environment]::SetEnvironmentVariable('Path', "$env:Path;$exePath", [System.EnvironmentVariableTarget]::User);
    $env:Path = [Environment]::GetEnvironmentVariable('Path', [System.EnvironmentVariableTarget]::User);
}

