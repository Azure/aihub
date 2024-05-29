resource "azurerm_container_app_environment" "cae" {
  name                                        = var.cae_name
  resource_group_name                         = var.resource_group_name
  location                                    = var.location
  dapr_application_insights_connection_string = var.appi_connection_string
  log_analytics_workspace_id                  = var.log_id
  infrastructure_subnet_id                    = var.cae_subnet_id
  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }
  tags = {}
}
