resource "azurerm_network_security_group" "nsg_cae" {
  name                = var.nsg_cae_name
  location            = var.location
  resource_group_name = var.resource_group_name
}

resource "azurerm_subnet_network_security_group_association" "nsg_cae_association" {
  subnet_id                 = var.cae_subnet_id
  network_security_group_id = azurerm_network_security_group.nsg_cae.id
}
