resource "azurerm_network_security_group" "nsg_pe" {
  name                = var.nsg_pe_name
  location            = var.location
  resource_group_name = var.resource_group_name
}

resource "azurerm_subnet_network_security_group_association" "nsg_pe_association" {
  subnet_id                 = var.pe_subnet_id
  network_security_group_id = azurerm_network_security_group.nsg_pe.id
}
