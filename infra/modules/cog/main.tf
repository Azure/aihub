resource "azurerm_cognitive_account" "form" {
  name                          = var.form_recognizer_name
  location                      = var.location
  resource_group_name           = var.resource_group_name
  kind                          = "FormRecognizer"
  sku_name                      = "S0"
  public_network_access_enabled = true
  custom_subdomain_name         = var.form_recognizer_name
  dynamic "network_acls" { # Only set network rules if private endpoints are used, adding allowed IPs to access the service
    for_each = var.use_private_endpoints ? [1] : []
      content {
        default_action = "Deny"
        ip_rules       = var.allowed_ips      
      }
  }
}

resource "azurerm_cognitive_account" "content_safety" {
  name                          = var.content_safety_name
  kind                          = "ContentSafety"
  sku_name                      = "S0"
  location                      = var.content_safety_location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.content_safety_name
  identity {
    type = "SystemAssigned"
  }
  dynamic "network_acls" { # Only set network rules if private endpoints are used, adding allowed IPs to access the service
    for_each = var.use_private_endpoints ? [1] : []
      content {
        default_action = "Deny"
        ip_rules       = var.allowed_ips      
      }
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
  dynamic "network_acls" { # Only set network rules if private endpoints are used, adding allowed IPs to access the service
    for_each = var.use_private_endpoints ? [1] : []
      content {
        default_action = "Deny"
        ip_rules       = var.allowed_ips      
      }
  }
}

resource "azurerm_cognitive_account" "speech" {
  name                          = var.speech_name
  kind                          = "SpeechServices"
  sku_name                      = "S0"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.speech_name
  dynamic "network_acls" { # Only set network rules if private endpoints are used, adding allowed IPs to access the service
    for_each = var.use_private_endpoints ? [1] : []
      content {
        default_action = "Deny"
        ip_rules       = var.allowed_ips      
      }
  }    
}

resource "azurerm_cognitive_account" "vision" {
  name                          = var.vision_name
  kind                          = "ComputerVision"
  sku_name                      = "S1"
  location                      = var.vision_location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.vision_name
  dynamic "network_acls" { # Only set network rules if private endpoints are used, adding allowed IPs to access the service
    for_each = var.use_private_endpoints ? [1] : []
      content {
        default_action = "Deny"
        ip_rules       = var.allowed_ips      
      }
  }
}

resource "azurerm_resource_group_template_deployment" "main" {
  count               = var.deploy_bing ? 1 : 0
  name                = var.bing_name
  resource_group_name = var.resource_group_name
  deployment_mode     = "Incremental"
  tags                = {}
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
          "type": "Object",
          "value": {
              "key1": "[listKeys(resourceId('Microsoft.Bing/accounts', parameters('name')), '2020-06-10').key1]",
              "key2": "[listKeys(resourceId('Microsoft.Bing/accounts', parameters('name')), '2020-06-10').key2]"
          }
      }
   }    
}
TEMPLATE
}

# Assign Cognitive Services identity to reader role on the storage account

resource "azurerm_role_assignment" "reader" {
  scope                = var.content_safety_storage_resource_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_cognitive_account.content_safety.identity[0].principal_id
}

## Private endpoints

resource "azurerm_private_dns_zone" "private_dns_zone_cognitive" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "privatelink.cognitiveservices.azure.com"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "private_dns_zone_link_cognitive" {
  count                 = var.use_private_endpoints ? 1 : 0
  name                  = var.content_safety_name
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.private_dns_zone_cognitive[0].name
  virtual_network_id    = var.vnet_id
}

resource "azurerm_private_endpoint" "pep_content_safety" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.content_safety_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.content_safety_name}-safety-privateserviceconnection"
    private_connection_resource_id = azurerm_cognitive_account.content_safety.id
    is_manual_connection           = false
    subresource_names              = ["account"]  
  }

  private_dns_zone_group {
    name                 = "${var.content_safety_name}-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_cognitive[0].id]
  }
}

resource "azurerm_private_endpoint" "pep_cognitive_services" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.cognitive_services_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.cognitive_services_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_cognitive_account.cognitive.id
    is_manual_connection           = false
    subresource_names              = ["account"]  
  }

  private_dns_zone_group {
    name                 = "${var.cognitive_services_name}-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_cognitive[0].id]
  }
}

resource "azurerm_private_endpoint" "pep_speech" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.speech_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.speech_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_cognitive_account.speech.id
    is_manual_connection           = false
    subresource_names              = ["account"]  
  }

  private_dns_zone_group {
    name                 = "${var.speech_name}-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_cognitive[0].id]
  }
}

resource "azurerm_private_endpoint" "pep_vision" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.vision_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.vision_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_cognitive_account.vision.id
    is_manual_connection           = false
    subresource_names              = ["account"]  
  }

  private_dns_zone_group {
    name                 = "${var.vision_name}-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_cognitive[0].id]
  }
}

resource "azurerm_private_endpoint" "pep_form" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.form_recognizer_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.form_recognizer_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_cognitive_account.form.id
    is_manual_connection           = false
    subresource_names              = ["account"]  
  }

  private_dns_zone_group {
    name                 = "${var.form_recognizer_name}-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_cognitive[0].id]
  }
}
