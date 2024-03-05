# Deploying the Solution

## Deploying the infrastructure

Run the following command to deploy the infrastructure:

```bash
az login
az account set -s <target subscription_id or subscription_name>
powershell -Command "iwr -useb https://raw.githubusercontent.com/azure/aihub/master/install/install.ps1 | iex"
```

## Manual steps
