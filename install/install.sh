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
mkdir -p "AIHubRoot" 2>/dev/null
if [[ ! -d "AIHubRoot" ]]; then
    echo "Warning: Please visit https://azure.github.io/aihub/docs/ for instructions on how to install without admin rights."
    echo "Cannot create AIHubRoot" >&2
    exit 1
fi

latest_aihub=$(curl -H githubHeader https://api.github.com/repos/Azure/aihub/releases/latest | jq -r .tag_name)