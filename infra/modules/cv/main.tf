resource "azurerm_cognitive_account" "cv" {
  name                = var.cv_name
  location            = var.location
  resource_group_name = var.resource_group_name
  kind                = "Face"
  sku_name            = "S0"
}