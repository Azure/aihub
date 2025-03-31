output "project_connection_string" {
  description = "The connection string to the AI Foundry project"
  value       = "${azurerm_ai_foundry_project.ai_foundry_project.location}.api.azureml.ms;${var.subscription_id};${var.resource_group_name};${azurerm_ai_foundry_project.ai_foundry_project.name}"
}

output "deployment_name" {
  value = azurerm_cognitive_deployment.gpt4o.name
}

output "bing_connection_name" {
  description = "The connection name to the Bing Search resource"
  value       = azapi_resource.bing_connection.name
}
