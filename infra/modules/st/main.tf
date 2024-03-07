resource "azurerm_storage_account" "sa" {
  name                            = var.storage_account_name
  location                        = var.location
  resource_group_name             = var.resource_group_name
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  enable_https_traffic_only       = true
  allow_nested_items_to_be_public = false
  # We are enabling the firewall only allowing traffic from our PC's public IP.
  #   network_rules {
  #     default_action             = "Deny"
  #     virtual_network_subnet_ids = []
  #     ip_rules = [
  #       jsondecode(data.http.current_public_ip.body).ip
  #     ]
  #   }
}

# Create data container
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

