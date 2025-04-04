echo ""
set -e
 
# GitHub Org and repo hosting AI Hub
GitHubOrg="azure"
GitHubRepo="aihub"

AIHubRoot="./.aihub"

# Set Github request authentication for basic authentication.
if [[ -n GITHUB_USER ]]; then
    basicAuth=$(echo -n "$GITHUB_USER:$GITHUB_TOKEN" | base64)
    githubHeader="Authorization: Basic $basicAuth"
else
    githubHeader=""
fi

echo "Installing AI Hub..."

# Create Dapr Directory
echo "Creating AIHubRoot directory"
mkdir -p "$AIHubRoot"
if [ ! -d "$AIHubRoot" ]; then
    echo "Warning: Please visit https://azure.github.io/aihub/docs/ for instructions on how to install without admin rights."
    echo "Cannot create $AIHubRoot" >&2
    exit 1
fi

latest_aihub=$(curl -H "$githubHeader" https://api.github.com/repos/Azure/aihub/releases/latest | jq -r .tag_name)
zipFileUrl="https://github.com/Azure/aihub/releases/download/$latest_aihub/aihub-tf-module.zip"
echo "Downloading $zipFileUrl ..."
zipFilePath="aihub-tf-module.zip" 
curl -H "$githubHeader" -H "Accept: application/octet-stream" -L $zipFileUrl --output $zipFilePath 

if [ ! -f "$zipFilePath" ]; then
    echo "Failed to download AI Hub - $zipFilePath" >&2
    exit 1
fi

# Extract AI Hub to AIHubRoot
echo "Extracting $zipFilePath..."
unzip -o $zipFilePath -d $AIHubRoot

# Move files to root
mv "$AIHubRoot/home/runner/work/aihub/aihub/release/aihub-tf-module/"* $AIHubRoot

# Clean up folder
rm -rf "$AIHubRoot/home"

# Clean up zipfile
echo "Clean up $zipFilePath..."
rm $zipFilePath

zipFilePath="terraform_1.7.4_linux_386.zip" 
zipFileUrl="https://releases.hashicorp.com/terraform/1.7.4/$zipFilePath"
echo "Downloading $zipFileUrl ..."
curl -L $zipFileUrl --output $zipFilePath

# Extract terraform to $AIHubRoot
echo "Extracting $zipFilePath..."
unzip -o $zipFilePath -d $AIHubRoot

# Clean up zipfile
echo "Clean up $zipFilePath..."
rm $zipFilePath

# Set subscription environment variable
echo "Setting up Azure subscription..."
export ARM_SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Use Terraform to deploy AI Hub
echo "Deploying AI Hub..."
cd $AIHubRoot
./terraform init 
./terraform apply -auto-approve
cd -

# Everything is done
echo -e "\nAI Hub deployed successfully."
echo "To get started with AI Hub, please visit https://azure.github.io/aihub ."
