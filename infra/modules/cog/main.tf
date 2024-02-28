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

resource "azapi_resource" "bing" {
  count                     = var.deploy_bing ? 1 : 0
  name                      = var.bing_name
  location                  = "global"
  parent_id                 = var.resource_group_id
  type                      = "Microsoft.Bing/accounts@2020-06-10"
  schema_validation_enabled = false // Required for this service otherwise it will fail.

  body = jsonencode({
    kind = "Bing.Search.v7"
    sku = {
      name = "S1"
    }
    properties : {
      statisticsEnabled = true
    }
  })
  response_export_values = ["properties.endpoint"]
}

resource "azurerm_role_assignment" "reader" {
  scope                = var.content_safety_storage_resource_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = azurerm_cognitive_account.content_safety.identity[0].principal_id
}
