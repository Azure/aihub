output "openai_service_name" {
  value = azurerm_cognitive_account.openai.name
}

output "openai_endpoint" {
  value = azurerm_cognitive_account.openai.endpoint
}

output "gpt_deployment_name" {
  value = azurerm_cognitive_deployment.gpt_35_turbo.name
}

output "embedding_deployment_name" {
  value = azurerm_cognitive_deployment.embedding.name
}

# output "secondary_openai_endpoint" {
#   value = azurerm_cognitive_account.secondary_openai.endpoint
# }
