output "apim_name" {
  value = var.enable_apim ? azurerm_api_management.apim[0].name : ""
}

output "gateway_url" {
  value = var.enable_apim ? azurerm_api_management.apim[0].gateway_url : ""
}
