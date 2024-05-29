output "fqdn" {
  value = azapi_resource.ca_back.output.properties.configuration.ingress.fqdn
}

