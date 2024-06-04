---
title: Deployment
description: AI Hub &mdash; Steps to deploy the complete solution.
weight: 4
---

# High-level Architecture

The following diagram shows the high-level architecture of the **AI Hub** solution:

![High-level Architecture](/aihub/img/AI-Hub-HLD.png)

## Deployment

To deploy the AI Hub on your own Azure subscription, please follow these instructions.

### Prerequisites

Before deploying the Azure AI Hub, please ensure the following prerequisites are met:

- Please review the values in the `variables.tf` file to create the appropriate `.tfvars` file with the values you prefer. Take into account that some elements in the architecture are optional, like for example private endpoints which by default are not deployed unless the value of the `use_private_endpoints` variable is set to `true`.
- You have Azure CLI version `2.56.0` or higher installed, or using the Azure Cloud Shell.
- During deployment, the script will create two application registrations on Microsoft Entra ID. Please verify that your user account has the necessary privileges.
- The Azure AI Hub uses various cognitive services like Azure Computer Vision, Azure Speech Service or Azure Document Intelligence. To deploy these Cognitive Services, you must manually accept the "Responsible AI" terms. This can currently only be done by deploying any of these services from the Azure Portal.


### Deploying the infrastructure - From Windows

Run the following command to deploy the infrastructure:

```bash
az login
az account set -s <target subscription_id or subscription_name>
powershell -Command "iwr -useb https://raw.githubusercontent.com/azure/aihub/master/install/install.ps1 | iex"
```

### Deploying the infrastructure - From Linux

For installation on Linux, we recommend using `Ubuntu 22.04` or a newer version. Before executing the installation script, ensure that the following applications are installed and up-to-date:

- `curl`, version `7.x` or higher.
- `jq`, version `1.6` or higher.
- `unzip`, version `6.x` or higher.

To deploy the infrastructure, execute the following command:

```bash
az login
az account set -s <target subscription_id or subscription_name>
bash -c "$(curl -fsSL https://raw.githubusercontent.com/azure/aihub/master/install/install_linux.sh)"
```
