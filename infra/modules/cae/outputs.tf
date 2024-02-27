output "cae_id" {
  value = azapi_resource.cae.id
}

output "defaultDomain" {
  value = jsondecode(azapi_resource.cae.output).properties.defaultDomain
}

