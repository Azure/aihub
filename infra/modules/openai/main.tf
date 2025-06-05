resource "azurerm_cognitive_account" "openai" {
  name                          = var.azopenai_name
  kind                          = "OpenAI"
  sku_name                      = "S0"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.azopenai_name
  dynamic "network_acls" { # Only set network rules if private endpoints are used, adding allowed IPs to access the service
    for_each = var.use_private_endpoints ? [1] : []
    content {
      default_action = "Deny"
      ip_rules       = var.allowed_ips
    }
  }
}

# Deploy models into Azure OpenAI

resource "azurerm_cognitive_deployment" "embedding" {
  name                 = "text-embedding-ada-002"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "text-embedding-ada-002"
    version = "2"
  }
  sku {
    name     = "Standard"
    capacity = 40
  }
}

resource "azurerm_cognitive_deployment" "gpt_4" {
  name                 = "gpt-4"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "gpt-4"
    version = "turbo-2024-04-09"
  }
  sku {
    name     = "Standard"
    capacity = 20
  }
}

resource "azurerm_cognitive_deployment" "gpt4_1" {
  name                 = "gpt4.1"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "gpt-4.1"
    version = "2025-04-14"
  }
  sku {
    name     = "GlobalStandard"
    capacity = 10
  }
}

resource "azurerm_cognitive_deployment" "gpt4o" {
  name                 = "gpt4o"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "gpt-4o"
    version = "2024-08-06"
  }
  sku {
    name     = "Standard"
    capacity = 30
  }
}

# Set role assignment for OpenAI

resource "azurerm_role_assignment" "openai_user" {
  scope                = azurerm_cognitive_account.openai.id
  role_definition_name = "Cognitive Services OpenAI User"
  principal_id         = var.principal_id
}

# Private endpoint

resource "azurerm_private_dns_zone" "private_dns_zone_openai" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "privatelink.openai.azure.com"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_endpoint" "pep_openai" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.azopenai_name}"
  location            = var.vnet_location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.azopenai_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_cognitive_account.openai.id
    is_manual_connection           = false
    subresource_names              = ["account"]
  }

  private_dns_zone_group {
    name                 = "${var.azopenai_name}-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_openai[0].id]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "private_dns_zone_link_openai" {
  count                 = var.use_private_endpoints ? 1 : 0
  name                  = var.azopenai_name
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.private_dns_zone_openai[0].name
  virtual_network_id    = var.vnet_id
}
