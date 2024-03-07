resource "azurerm_cognitive_account" "content_safety" {
  name                          = var.content_safety_name
  kind                          = "ContentSafety"
  sku_name                      = "S0"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.content_safety_name
  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_cognitive_account" "cognitive" {
  name                          = var.cognitive_services_name
  kind                          = "CognitiveServices"
  sku_name                      = "S0"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.cognitive_services_name
}

resource "azurerm_cognitive_account" "speech" {
  name                          = var.speech_name
  kind                          = "SpeechServices"
  sku_name                      = "S0"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.speech_name
}

resource "azurerm_resource_group_template_deployment" "main" {
  name                = var.bing_name
  resource_group_name = var.resource_group_name
  deployment_mode     = "Incremental"
  parameters_content = jsonencode({
    "name" = {
      value = var.bing_name
    },
    "location" = {
      value = "Global"
    },
    "sku" = {
      value = "S1"
    },
    "kind" = {
      value = "Bing.Search.v7"
    }
  })
  template_content = <<TEMPLATE
{
    "$schema": "http://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "name": {
            "type": "String"
        },
        "location": {
            "type": "String"
        },
        "sku": {
            "type": "String"
        },
        "kind": {
          "type": "String"
        }

    },
    "resources": [
        {
            "apiVersion": "2020-06-10",
            "name": "[parameters('name')]",
            "location": "[parameters('location')]",
            "type": "Microsoft.Bing/accounts",
            "kind": "[parameters('kind')]",
            "sku": {
                "name": "[parameters('sku')]"
            }
        }
    ],
    "outputs": {
      "accessKeys": {
          "type": "object",
          "value": {
              "key1": "[listKeys(resourceId('Microsoft.Bing/accounts', parameters('name')), '2020-06-10').key1]",
              "key2": "[listKeys(resourceId('Microsoft.Bing/accounts', parameters('name')), '2020-06-10').key2]"
          }
      }
   }    
}
TEMPLATE
}

resource "azurerm_role_assignment" "reader" {
  scope                = var.content_safety_storage_resource_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_cognitive_account.content_safety.identity[0].principal_id
}
