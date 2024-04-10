resource "azurerm_storage_account" "sa" {
  name                      = "stfunc${var.func_name}"
  location                  = var.location
  resource_group_name       = var.resource_group_name
  account_tier              = "Standard"
  account_replication_type  = "LRS"
  enable_https_traffic_only = true
}

resource "azapi_resource" "ca_function" {
  schema_validation_enabled = false
  name                      = "func-${var.func_name}"
  location                  = var.location
  parent_id                 = var.resource_group_id
  type                      = "Microsoft.Web/sites@2023-01-01"
  body = jsonencode({
    kind = "functionapp,linux,container,azurecontainerapps"
    properties : {
      language             = "dotnet-isolated"
      managedEnvironmentId = "${var.cae_id}"
      siteConfig = {
        linuxFxVersion = "DOCKER|cmendibl3/aoai-plugin:0.8.0"
        appSettings = [
          {
            name  = "AzureWebJobsStorage"
            value = azurerm_storage_account.sa.primary_connection_string
          },
          {
            name  = "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"
            value = azurerm_storage_account.sa.primary_connection_string
          },
          {
            name  = "APPINSIGHTS_INSTRUMENTATIONKEY"
            value = var.appi_instrumentation_key
          },
          {
            name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
            value = "InstrumentationKey=${var.appi_instrumentation_key}"
          },
          {
            name  = "FUNCTIONS_WORKER_RUNTIME"
            value = "dotnet-isolated"
          },
          {
            name  = "FUNCTIONS_EXTENSION_VERSION"
            value = "~4"
          },
          {
            name  = "MODEL_ID"
            value = var.openai_model
          },
          {
            name  = "API_KEY"
            value = var.openai_key
          },
          {
            name  = "ENDPOINT"
            value = var.openai_endpoint
          },
          {
            name  = "OpenApi__HostNames"
            value = "https://func-${var.func_name}.${var.cae_default_domain}/api"
          }
        ]
      }
      workloadProfileName = "Consumption"
      resourceConfig = {
        cpu    = 1
        memory = "2Gi"
      }
      httpsOnly = false
    }
  })
}
