output "virtual_network_name" {
  value = azurerm_virtual_network.vnet.name
}

output "apim_subnet_id" {
  value = azurerm_subnet.apim.id
}

output "cae_subnet_id" {
  value = azurerm_subnet.cae.id
}

output "pe_subnet_id" {
  value = azurerm_subnet.private_endpoints.id
}
