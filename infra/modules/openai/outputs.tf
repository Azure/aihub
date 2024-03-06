output "openai_service_name" {
  value = azurerm_cognitive_account.openai.name
}

output "openai_endpoint" {
  value = azurerm_cognitive_account.openai.endpoint
}

output "gpt_deployment_name" {
  value = azurerm_cognitive_deployment.gpt_35_turbo.name
}

output "gpt_deployment_model_name" {
  value = azurerm_cognitive_deployment.gpt_35_turbo.model[0].name
}

output "gpt4_vision_deployment_name" {
  value = azurerm_cognitive_deployment.gpt4_vision.name
}

output "gpt4_vision_deployment_model_name" {
  value = azurerm_cognitive_deployment.gpt4_vision.model[0].name
}

output "gpt4_deployment_name" {
  value = azurerm_cognitive_deployment.gpt4.name
}

output "gpt4_deployment_model_name" {
  value = azurerm_cognitive_deployment.gpt4.model[0].name
}

output "embedding_deployment_name" {
  value = azurerm_cognitive_deployment.embedding.name
}


# output "secondary_openai_endpoint" {
#   value = azurerm_cognitive_account.secondary_openai.endpoint
# }
