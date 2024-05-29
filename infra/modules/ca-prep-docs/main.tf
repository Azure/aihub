resource "azapi_resource" "ca_back" {
  name      = var.ca_name
  location  = var.location
  parent_id = var.resource_group_id
  type      = "Microsoft.App/jobs@2023-05-01"
  identity {
    type = "UserAssigned"
    identity_ids = [
      var.managed_identity_id
    ]
  }

  body = {
    properties : {
      environmentId = "${var.cae_id}"
      configuration = {
        manualTriggerConfig = {
          parallelism            = 1
          replicaCompletionCount = 1
        }
        secrets           = []
        triggerType       = "Manual"
        replicaTimeout    = 3600
        replicaRetryLimit = 0
      }
      template = {
        containers = [
          {
            name  = "aihub-prepdocs"
            image = "${var.image_name}"
            resources = {
              cpu    = 0.5
              memory = "1Gi"
            }
            env = [
              {
                name  = "OPENAI_HOST"
                value = "azure"
              },
              {
                name  = "AZURE_OPENAI_EMB_DEPLOYMENT"
                value = "text-embedding-ada-002"
              },
              {
                name  = "AZURE_OPENAI_EMB_MODEL_NAME"
                value = "text-embedding-ada-002"
              },
              {
                name  = "AZURE_STORAGE_CONTAINER"
                value = "content"
              },
              {
                name  = "AZURE_SEARCH_INDEX"
                value = "gptkbindex"
              },
              {
                name  = "OPENAI_API_KEY"
                value = ""
              },
              {
                name  = "OPENAI_ORGANIZATION"
                value = ""
              },
              {
                name  = "AZURE_RESOURCE_GROUP"
                value = "${var.resource_group_name}"
              },
              {
                name  = "AZURE_SUBSCRIPTION_ID"
                value = "${var.subscription_id}"
              },
              {
                name  = "AZURE_STORAGE_ACCOUNT"
                value = "${var.storage_account_name}"
              },
              {
                name  = "AZURE_SEARCH_SERVICE"
                value = "${var.search_service_name}"
              },
              {
                name  = "AZURE_OPENAI_SERVICE"
                value = "${var.openai_service_name}"
              },
              {
                name  = "AZURE_TENANT_ID"
                value = "${var.tenant_id}"
              },
              {
                name  = "AZURE_CLIENT_ID"
                value = "${var.managed_identity_client_id}"
              },
            ],
            volumeMounts = [
              {
                volumeName = "staging-volume"
                mountPath  = "/data"
              }
            ]
          },
        ]
        volumes = [
          {
            name        = "staging-volume"
            storageName = "${azurerm_container_app_environment_storage.data.name}"
            storageType = "AzureFile"
          }
        ]
      }
    }
  }
  response_export_values = ["properties.configuration.ingress.fqdn"]
}

resource "azurerm_container_app_environment_storage" "data" {
  name                         = "stagingstorage"
  container_app_environment_id = var.cae_id
  account_name                 = var.storage_account_name
  share_name                   = "staging"
  access_key                   = var.storage_account_key
  access_mode                  = "ReadWrite"
}
