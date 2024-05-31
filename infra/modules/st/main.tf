locals {
  network_rules_bypass = var.use_private_endpoints ? [ "None" ] : [ "AzureServices" ]
}

resource "azurerm_storage_account" "sa" {
  name                            = var.storage_account_name
  location                        = var.location
  resource_group_name             = var.resource_group_name
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  enable_https_traffic_only       = true
  allow_nested_items_to_be_public = false
}

resource "azurerm_storage_account_network_rules" "sa_network_rules" {
  count                      = var.use_private_endpoints ? 1 : 0
  storage_account_id         = azurerm_storage_account.sa.id
  default_action             = "Deny"
  virtual_network_subnet_ids = []
  ip_rules                   = var.allowed_ips
  bypass                     = local.network_rules_bypass
}

# Create containers and file shares, then populate them as required.

resource "azurerm_storage_container" "content" {
  name                  = "content"
  container_access_type = "private"
  storage_account_name  = azurerm_storage_account.sa.name
}

resource "azurerm_storage_container" "audio" {
  name                  = "audio-files"
  container_access_type = "private"
  storage_account_name  = azurerm_storage_account.sa.name
}

resource "azurerm_storage_container" "form-analyzer" {
  name                  = "form-analyzer"
  container_access_type = "private"
  storage_account_name  = azurerm_storage_account.sa.name
}

resource "azurerm_storage_container" "image-analyzer" {
  name                  = "image-analyzer"
  container_access_type = "private"
  storage_account_name  = azurerm_storage_account.sa.name
}

resource "azurerm_storage_container" "video-analyzer" {
  name                  = "video-analyzer"
  container_access_type = "private"
  storage_account_name  = azurerm_storage_account.sa.name
}

resource "azurerm_storage_container" "image-moderator" {
  name                  = "image-moderator"
  container_access_type = "private"
  storage_account_name  = azurerm_storage_account.sa.name
}

resource "azurerm_storage_container" "document-comparison" {
  name                  = "document-comparison"
  container_access_type = "private"
  storage_account_name  = azurerm_storage_account.sa.name
}

resource "azurerm_storage_share" "share" {
  name                 = "staging"
  storage_account_name = azurerm_storage_account.sa.name
  quota                = 5
}

resource "azurerm_storage_share" "customization" {
  name                 = "customization"
  storage_account_name = azurerm_storage_account.sa.name
  quota                = 5
}

resource "azurerm_storage_share_file" "docs" {
  for_each         = fileset("${path.module}/docs", "*")
  name             = each.value
  storage_share_id = azurerm_storage_share.share.id
  source           = "${path.module}/docs/${each.value}"
  content_md5      = filemd5("${path.module}/docs/${each.value}")
}

resource "azurerm_storage_share_file" "customization" {
  for_each         = fileset("${path.module}/customization/customer", "*")
  name             = each.value
  storage_share_id = azurerm_storage_share.customization.id
  source           = "${path.module}/customization/customer/${each.value}"
  content_md5      = filemd5("${path.module}/customization/customer/${each.value}")
}

resource "azurerm_role_assignment" "storage_contributor" {
  scope                = azurerm_storage_account.sa.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.principal_id
}

# Private endpoint for the Blob Storage

resource "azurerm_private_dns_zone" "private_dns_zone_blob" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_endpoint" "pep_blob" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.storage_account_name}-blob"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.storage_account_name}-blob-privateserviceconnection"
    private_connection_resource_id = azurerm_storage_account.sa.id
    is_manual_connection           = false
    subresource_names              = ["blob"]
  }

  private_dns_zone_group {
    name                 = "${var.storage_account_name}-blob-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_blob[0].id]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "private_dns_zone_link_blob" {
  count                 = var.use_private_endpoints ? 1 : 0
  name                  = "${var.storage_account_name}-blob"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.private_dns_zone_blob[0].name
  virtual_network_id    = var.vnet_id
}

# Private endpoint for the File Share

resource "azurerm_private_dns_zone" "private_dns_zone_file" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "privatelink.file.core.windows.net"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_endpoint" "pep_file" {
  count               = var.use_private_endpoints ? 1 : 0
  name                = "pep-${var.storage_account_name}-file"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.private_endpoints_subnet_id

  private_service_connection {
    name                           = "${var.storage_account_name}-file-privateserviceconnection"
    private_connection_resource_id = azurerm_storage_account.sa.id
    is_manual_connection           = false
    subresource_names              = ["file"]
  }

  private_dns_zone_group {
    name                 = "${var.storage_account_name}-file-privatelink"
    private_dns_zone_ids = [azurerm_private_dns_zone.private_dns_zone_file[0].id]
  }
}

resource "azurerm_private_dns_zone_virtual_network_link" "private_dns_zone_link_file" {
  count               = var.use_private_endpoints ? 1 : 0
  name                  = "file"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.private_dns_zone_file[0].name
  virtual_network_id    = var.vnet_id
}
