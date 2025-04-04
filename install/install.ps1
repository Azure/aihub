Write-Output ""
$ErrorActionPreference = 'stop'

# GitHub Org and repo hosting AI Hub
$GitHubOrg = "azure"
$GitHubRepo = "aihub"

$AIHubRoot = "./.aihub"

# Set Github request authentication for basic authentication.
if ($Env:GITHUB_USER) {
    $basicAuth = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($Env:GITHUB_USER + ":" + $Env:GITHUB_TOKEN));
    $githubHeader = @{"Authorization" = "Basic $basicAuth" }
}
else {
    $githubHeader = @{}
}

if ((Get-ExecutionPolicy) -gt 'RemoteSigned' -or (Get-ExecutionPolicy) -eq 'ByPass') {
    Write-Output "PowerShell requires an execution policy of 'RemoteSigned'."
    Write-Output "To make this change please run:"
    Write-Output "'Set-ExecutionPolicy RemoteSigned -scope CurrentUser'"
    break
}

# Change security protocol to support TLS 1.2 / 1.1 / 1.0 - old powershell uses TLS 1.0 as a default protocol
[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"

Write-Output "Installing AI Hub..."

# Create Dapr Directory
Write-Output "Creating AIHubRoot directory"
New-Item -ErrorAction Ignore -Path $AIHubRoot -ItemType "directory"
if (!(Test-Path $AIHubRoot -PathType Container)) {
    Write-Warning "Please visit https://azure.github.io/aihub/docs/ for instructions on how to install without admin rights."
    throw "Cannot create $AIHubRoot"
}

$latest_aihub=$(Invoke-WebRequest -Headers $githubHeader -Uri https://api.github.com/repos/Azure/aihub/releases/latest).content | convertfrom-json | Select-Object -ExpandProperty tag_name
$zipFileUrl = "https://github.com/Azure/aihub/releases/download/$latest_aihub/aihub-tf-module.zip"
Write-Output "Downloading $zipFileUrl ..."
$zipFilePath = "aihub-tf-module.zip" 
$githubHeader.Accept = "application/octet-stream"
Invoke-WebRequest -Headers $githubHeader -Uri $zipFileUrl -OutFile $zipFilePath 

if (!(Test-Path $zipFilePath -PathType Leaf)) {
    throw "Failed to download AI Hub - $zipFilePath"
}

# Extract AI Hub to $AIHubRoot
Write-Output "Extracting $zipFilePath..."
Microsoft.Powershell.Archive\Expand-Archive -Force -Path $zipFilePath -DestinationPath $AIHubRoot

# Move files to root
Move-Item -Path "$AIHubRoot\home\runner\work\aihub\aihub\release\aihub-tf-module\**" -Destination $AIHubRoot -Force

# Clean up folder
Remove-Item "$AIHubRoot\home" -Force -Recurse

# Clean up zipfile
Write-Output "Clean up $zipFilePath..."
Remove-Item $zipFilePath -Force

$zipFileUrl= "https://releases.hashicorp.com/terraform/1.7.4/terraform_1.7.4_windows_386.zip"
$zipFilePath = "aihub-tf-module.zip"
Write-Output "Downloading $zipFileUrl ..."
$zipFilePath = "terraform_1.7.4_windows_386.zip" 
Invoke-WebRequest -Uri $zipFileUrl -OutFile $zipFilePath

# Extract terraform to $AIHubRoot
Write-Output "Extracting $zipFilePath..."
Microsoft.Powershell.Archive\Expand-Archive -Force -Path $zipFilePath -DestinationPath $AIHubRoot

# Clean up zipfile
Write-Output "Clean up $zipFilePath..."
Remove-Item $zipFilePath -Force

# Set subscription environment variable
Write-Output  "Setting up Azure subscription..."
$Env:ARM_SUBSCRIPTION_ID = (az account show --query id -o tsv)

# Use Terraform to deploy AI Hub
Write-Output "Deploying AI Hub..."
Push-Location $AIHubRoot
Invoke-Expression "./terraform init" 
Invoke-Expression "./terraform apply -auto-approve"
Pop-Location

# Everything is done
Write-Output "`r`nAI Hub deployed successfully."
Write-Output "To get started with AI HUb, please visit https://azure.github.io/aihub ."