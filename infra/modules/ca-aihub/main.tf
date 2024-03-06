resource "azapi_resource" "ca_back" {
  name      = var.ca_name
  location  = var.location
  parent_id = var.resource_group_id
  type      = "Microsoft.App/containerApps@2023-05-01"
  identity {
    type = "UserAssigned"
    identity_ids = [
      var.managed_identity_id
    ]
  }

  body = jsonencode({
    properties : {
      managedEnvironmentId = "${var.cae_id}"
      configuration = {
        secrets = [
          {
            name  = "microsoft-provider-authentication-secret"
            value = "${var.enable_entra_id_authentication ? module.sp[0].password : "None"}"
          },
          {
            name  = "content-safety-key"
            value = "${var.content_safety_key}"
          },
          {
            name  = "cognitive-service-key",
            value = "${var.cognitive_service_key}"
          },
          {
            name  = "speech-key",
            value = "${var.speech_key}"
          },
          {
            name  = "storage-connection-string"
            value = "${var.storage_connection_string}"
          },
          {
            name  = "bing-key"
            value = "${var.bing_key}"
          }
        ]
        ingress = {
          external   = true
          targetPort = 8080
          transport  = "Http"

          traffic = [
            {
              latestRevision = true
              weight         = 100
            }
          ]
        }
        dapr = {
          enabled = false
        }
      }
      template = {
        containers = [
          {
            name  = "aihub"
            image = "${var.image_name}"
            resources = {
              cpu    = 0.5
              memory = "1Gi"
            }
            env = [
              {
                name  = "Logging__LogLevel__Default",
                value = "Information"
              },
              {
                name  = "Logging__LogLevel__Microsoft.AspNetCore",
                value = "Warning"
              },
              {
                name  = "ContentModerator__Endpoint",
                value = "${var.content_safety_endpoint}"
              },
              {
                name      = "ContentModerator__SubscriptionKey",
                secretRef = "content-safety-key"
              },
              {
                name  = "ContentModerator__JailbreakDetectionEndpoint",
                value = "contentsafety/text:detectJailbreak?api-version=2023-10-15-preview"
              },
              {
                name  = "BrandAnalyzer__BingEndpoint",
                value = "https://api.bing.microsoft.com/v7.0/search"
              },
              {
                name      = "BrandAnalyzer__BingKey",
                secretRef = "bing-key"
              },
              {
                name  = "BrandAnalyzer__OpenAIEndpoint",
                value = "${var.openai_endpoint}"
              },
              {
                name  = "BrandAnalyzer__OpenAISubscriptionKey",
                value = ""
              },
              {
                name  = "BrandAnalyzer__DeploymentName",
                value = var.chat_gpt_deployment
              },
              {
                name  = "CallCenter__OpenAIEndpoint",
                value = "${var.openai_endpoint}"
              },
              {
                name  = "CallCenter__OpenAISubscriptionKey",
                value = ""
              },
              {
                name  = "CallCenter__DeploymentName",
                value = var.chat_gpt_deployment
              },
              {
                name  = "ImageAnalyzer__OpenAIEndpoint",
                value = "${var.openai_endpoint}"
              },
              {
                name  = "ImageAnalyzer__OpenAISubscriptionKey",
                value = ""
              },
              {
                name  = "ImageAnalyzer__ContainerName",
                value = "image-analyzer"
              },
              {
                name  = "ImageAnalyzer__DeploymentName",
                value = var.chat_gpt4_vision_deployment
              },
              {
                name  = "FormAnalyzer__FormRecogEndpoint",
                value = "${var.cognitive_service_endpoint}"
              },
              {
                name      = "FormAnalyzer__FormRecogSubscriptionKey",
                secretRef = "cognitive-service-key"
              },
              {
                name  = "FormAnalyzer__OpenAIEndpoint",
                value = "${var.openai_endpoint}"
              },
              {
                name  = "FormAnalyzer__OpenAISubscriptionKey",
                value = ""
              },
              {
                name  = "FormAnalyzer__ContainerName",
                value = "form-analyzer"
              },
              {
                name  = "FormAnalyzer__DeploymentName",
                value = var.chat_gpt4_deployment
              },
              {
                name  = "DocumentComparison__FormRecogEndpoint",
                value = "${var.cognitive_service_endpoint}"
              },
              {
                name      = "DocumentComparison__FormRecogSubscriptionKey",
                secretRef = "cognitive-service-key"
              },
              {
                name  = "DocumentComparison__OpenAIEndpoint",
                value = "${var.openai_endpoint}"
              },
              {
                name  = "DocumentComparison__OpenAISubscriptionKey",
                value = ""
              },
              {
                name  = "DocumentComparison__ContainerName",
                value = "document-comparison"
              },
              {
                name  = "DocumentComparison__DeploymentName",
                value = var.chat_gpt4_deployment
              },
              {
                name      = "Storage__ConnectionString",
                secretRef = "storage-connection-string"
              },
              {
                name  = "Storage__ContainerName",
                value = "image-moderator"
              },
              {
                name  = "AudioTranscription__SpeechLocation",
                value = "${var.location}"
              },
              {
                name      = "AudioTranscription__SpeechSubscriptionKey",
                secretRef = "speech-key"
              },
              {
                name  = "AudioTranscription__ContainerName",
                value = "audio-files"
              },
              {
                name  = "ChatOnYourData__Link",
                value = "https://${var.chat_fqdn}"
              },
              {
                name  = "PBIReport__Link",
                value = "${var.pbi_report_link}"
              },
              {
                name  = "AllowedHosts",
                value = "*"
              },
              {
                name  = "AZURE_TENANT_ID"
                value = "${var.tenant_id}"
              },
              {
                name  = "AZURE_CLIENT_ID"
                value = "${var.managed_identity_client_id}"
              },
              {
                name  = "ASPNETCORE_ENVIRONMENT"
                value = "Development"
              }
            ],
            volumeMounts = [
              {
                volumeName = "customization-volume"
                mountPath  = "/wwwroot/images/customer"
              }
            ]
          },
        ]
        scale = {
          minReplicas = 1
          maxReplicas = 1
        },
        volumes = [
          {
            name        = "customization-volume"
            storageName = "${azurerm_container_app_environment_storage.data.name}"
            storageType = "AzureFile"
          }
        ]
      }
    }
  })
  response_export_values = ["properties.configuration.ingress.fqdn"]
}

resource "azurerm_container_app_environment_storage" "data" {
  name                         = "customizationstorage"
  container_app_environment_id = var.cae_id
  account_name                 = var.storage_account_name
  share_name                   = "customization"
  access_key                   = var.storage_account_key
  access_mode                  = "ReadWrite"
}
