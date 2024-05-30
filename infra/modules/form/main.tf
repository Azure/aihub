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

# Private endpoint

resource "azurerm_private_dns_zone" "private_dns_zone_form" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "privatelink.${var.form_recognizer_name}.azure.com"
  resource_group_name = var.resource_group_name
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
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_form[0].id]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "private_dns_zone_link_form" {
  count                 = var.use_private_endpoints ? 1 : 0
  name                  = var.form_recognizer_name
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.private_dns_zone_form[0].name
  virtual_network_id    = var.vnet_id
}
