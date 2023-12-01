---
title: Deployment
description: AI Hub &mdash; Steps to deploy the complete solution.
weight: 1
---

# Create a new resource group
az group create --name aihub-rg --location westeurope

# Create a new storage account
az storage account create --name aihubstorage --resource-group aihub-rg --location westeurope --sku Standard_LRS

# Create a new app service plan
az appservice plan create --name aihub-appservice --resource-group aihub-rg --sku S1

# Create a new web app using dot net 8
az webapp create --name aihub-webapp --resource-group aihub-rg --plan aihub-appservice --runtime "DOTNET|8.0"

# Create an Azure multi service cognitive service account
az cognitiveservices account create --name aihub-cognitiveservice --resource-group aihub-rg  --kind CognitiveServices --sku S0 --location westeurope --yes

# Create an Azure OpenAI resuorce
az openai create --name aihub-openai --resource-group aihub-rg --location westeurope

# Create a Content Safety Cognitive Service
az cognitiveservices account create --name aihub-contentsafety --resource-group aihub-rg --kind ContentSafety --sku F0 --location westeurope

# Deploy chat using GEN-AI VBD material
[Activate GenAI with Azure](https://azure.github.io/activate-genai/).

# Configure the web app
As a last step, we need to configure the web app to use the storage account and the cognitive service account we created earlier. To do this, we need to rename appsettings.json.template file to appsettings.json and replace the placeholders with the values we got from the previous steps.