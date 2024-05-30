resource "azurerm_search_service" "search" {
  name                = var.search_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "standard"
  semantic_search_sku = "free"
  allowed_ips          = var.use_private_endpoints ? var.allowed_ips : null

  local_authentication_enabled = false
}

resource "azurerm_role_assignment" "search_reader" {
  scope                = azurerm_search_service.search.id
  role_definition_name = "Search Index Data Reader"
  principal_id         = var.principal_id
}

resource "azurerm_role_assignment" "search_data_contributor" {
  scope                = azurerm_search_service.search.id
  role_definition_name = "Search Index Data Contributor"
  principal_id         = var.principal_id
}

resource "azurerm_role_assignment" "search_service_contributor" {
  scope                = azurerm_search_service.search.id
  role_definition_name = "Search Service Contributor"
  principal_id         = var.principal_id
}

# Private endpoint

resource "azurerm_private_dns_zone" "private_dns_zone_search" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "privatelink.${var.search_name}.azure.com"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_endpoint" "pep_search" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.search_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.search_name}-privateserviceconnection"
    private_connection_resource_id = azurerm_search_service.search.id
    is_manual_connection           = false
    subresource_names              = ["searchService"]  
  }

  private_dns_zone_group {
    name                 = "${var.search_name}-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_search[0].id]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "private_dns_zone_link_search" {
  count                 = var.use_private_endpoints ? 1 : 0
  name                  = var.search_name
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.private_dns_zone_search[0].name
  virtual_network_id    = var.vnet_id
}
