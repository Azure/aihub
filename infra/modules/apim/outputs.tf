output "apim_name" {
  value = azapi_resource.apim.name
}

output "gateway_url" {
  value = jsondecode(azapi_resource.apim.output).properties.gatewayUrl
}