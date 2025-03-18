output "openai_service_name" {
  value = azurerm_cognitive_account.openai.name
}

output "openai_endpoint" {
  value = azurerm_cognitive_account.openai.endpoint
}

output "gpt4_vision_deployment_name" {
  value = azurerm_cognitive_deployment.gpt4_vision.name
}

output "gpt4_vision_deployment_model_name" {
  value = azurerm_cognitive_deployment.gpt4_vision.model[0].name
}

output "gpt4_deployment_name" {
  value = azurerm_cognitive_deployment.gpt_4.name
}

output "gpt4_deployment_model_name" {
  value = azurerm_cognitive_deployment.gpt_4.model[0].name
}

output "gpt4o_deployment_name" {
  value = azurerm_cognitive_deployment.gpt4o.name
}

output "gpt4o_deployment_model_name" {
  value = azurerm_cognitive_deployment.gpt4o.model[0].name
}

output "embedding_deployment_name" {
  value = azurerm_cognitive_deployment.embedding.name
}

output "openai_key" {
  value = azurerm_cognitive_account.openai.primary_access_key
}
