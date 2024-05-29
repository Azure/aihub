output "cae_id" {
  value = azapi_resource.cae.id
}

output "default_domain" {
  value = azapi_resource.cae.output.properties.defaultDomain
}

