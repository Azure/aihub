output "fqdn" {
  value = jsondecode(azapi_resource.ca_back.output).properties.configuration.ingress.fqdn
}

