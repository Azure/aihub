output "log_id" {
  value = azurerm_log_analytics_workspace.logs.id
}

output "log_workspace_id" {
  value = azurerm_log_analytics_workspace.logs.workspace_id
}

output "log_key" {
  value = azurerm_log_analytics_workspace.logs.primary_shared_key
}
