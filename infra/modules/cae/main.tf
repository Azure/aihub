resource "azapi_resource" "cae" {
  name      = var.cae_name
  location  = var.location
  parent_id = var.resource_group_id
  type      = "Microsoft.App/managedEnvironments@2022-11-01-preview"

  body = {
    properties : {
      daprAIInstrumentationKey = "${var.appi_key}"
      appLogsConfiguration = {
        destination = "log-analytics"
        logAnalyticsConfiguration = {
          customerId = "${var.log_workspace_id}"
          sharedKey  = "${var.log_key}"
        }
      }
      vnetConfiguration = {
        internal               = false
        infrastructureSubnetId = "${var.cae_subnet_id}"
      }
      workloadProfiles = [
        {
          workloadProfileType = "Consumption"
          name                = "Consumption"
        },
      ]
    }
  }
  response_export_values = ["properties.defaultDomain"]
}
