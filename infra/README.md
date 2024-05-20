# Deploying the Solution

## Prerequisites

Before deploying and installing the Azure AI Hub, please ensure the following prerequisites are met:

- You have Azure CLI version `2.56.0` or higher installed.
- During deployment, the script will create two application registrations on Microsoft Entra ID. Please verify that your user account has the necessary privileges.
- The Azure AI Hub uses various cognitive services like Azure Computer Vision, Azure Speech Service or Azure Document Intelligence. To deploy these Cognitive Services, you must manually accept the "Responsible AI" terms. This can currently only be done by deploying any of these services from the Azure Portal.

## Deploying the infrastructure - Windows

Run the following command to deploy the infrastructure:

```bash
az login
az account set -s <target subscription_id or subscription_name>
powershell -Command "iwr -useb https://raw.githubusercontent.com/azure/aihub/master/install/install.ps1 | iex"
```

## Deploying the infrastructure - Linux

For installation on Linux, we recommend using `Ubuntu 22.04` or a newer version. Before executing the installation script, ensure that the following applications are installed and up-to-date:

- `curl`, version `7.x` or higher
- `jq`, version `1.6` or higher

To deploy the infrastructure, execute the following command:

```bash
az login
az account set -s <target subscription_id or subscription_name>
bash -c "$(curl -fsSL https://raw.githubusercontent.com/azure/aihub/master/install/install_linux.sh)"
```

## Manual steps

For details on manual deployment, please refer to the [Deployment](https://azure.github.io/aihub/docs/deployment/) section in the Azure AI Hub documentation.
