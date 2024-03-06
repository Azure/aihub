locals {
  logger_name = "openai-appi-logger"
  backend_url = "${var.openai_service_endpoint}openai"
}

resource "azurerm_public_ip" "apim_public_ip" {
  name                = "pip-apim"
  location            = var.location
  resource_group_name = var.resource_group_name
  allocation_method   = "Static"
  sku                 = "Standard"
  ip_tags             = {}
  zones               = ["1", "2", "3"]
  domain_name_label   = var.apim_name
}

resource "azurerm_api_management" "apim" {
  name                 = var.apim_name
  location             = var.location
  resource_group_name  = var.resource_group_name
  publisher_name       = var.publisher_name
  publisher_email      = var.publisher_email
  sku_name             = "Developer_1"
  virtual_network_type = "External"                          # Use "Internal" for a fully private APIM
  public_ip_address_id = azurerm_public_ip.apim_public_ip.id // Required to deploy APIM in STv2 platform

  virtual_network_configuration {
    subnet_id = var.apim_subnet_id
  }
}

resource "azurerm_api_management_backend" "openai" {
  name                = "openai-api"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.apim.name
  protocol            = "http"
  url                 = local.backend_url
  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }
}

resource "azurerm_api_management_logger" "appi_logger" {
  name                = local.logger_name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name
  resource_id         = var.appi_resource_id

  application_insights {
    instrumentation_key = var.appi_instrumentation_key
  }
}

// https://learn.microsoft.com/en-us/semantic-kernel/deploy/use-ai-apis-with-api-management#setup-azure-api-management-instance-with-azure-openai-api
resource "azurerm_api_management_api" "openai" {
  name                  = "openai-api"
  resource_group_name   = var.resource_group_name
  api_management_name   = azurerm_api_management.apim.name
  revision              = "1"
  display_name          = "Azure Open AI API"
  path                  = "openai"
  protocols             = ["https"]
  subscription_required = false
  service_url           = local.backend_url

  import {
    content_format = "openapi-link"
    content_value  = "https://raw.githubusercontent.com/Azure/azure-rest-api-specs/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference/stable/2023-05-15/inference.json"
  }
}

resource "azurerm_api_management_named_value" "tenant_id" {
  name                = "tenant-id"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.apim.name
  display_name        = "TENANT_ID"
  value               = var.tenant_id
}

resource "azurerm_api_management_api_policy" "policy" {
  api_name            = azurerm_api_management_api.openai.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = var.resource_group_name

  xml_content = <<XML
    <policies>
        <inbound>
            <base />
            <validate-jwt header-name="Authorization" failed-validation-httpcode="403" failed-validation-error-message="Forbidden">
                <openid-config url="https://login.microsoftonline.com/{{TENANT_ID}}/v2.0/.well-known/openid-configuration" />
                <issuers>
                    <issuer>https://sts.windows.net/{{TENANT_ID}}/</issuer>
                </issuers>
                <required-claims>
                    <claim name="aud">
                        <value>https://cognitiveservices.azure.com</value>
                    </claim>
                </required-claims>
            </validate-jwt>

            <set-backend-service backend-id="openai-api" />
        </inbound>
        <backend>
          <base />
        </backend>
        <outbound>
            <base />
        </outbound>
        <on-error>
          <base />
        </on-error>
    </policies>
    XML
  depends_on  = [azurerm_api_management_backend.openai]
}

# https://github.com/aavetis/azure-openai-logger/blob/main/README.md
resource "azurerm_api_management_diagnostic" "diagnostics" {
  identifier               = "applicationinsights"
  resource_group_name      = var.resource_group_name
  api_management_name      = azurerm_api_management.apim.name
  api_management_logger_id = azurerm_api_management_logger.appi_logger.id

  sampling_percentage       = 100
  always_log_errors         = true
  log_client_ip             = false
  verbosity                 = "information"
  http_correlation_protocol = "W3C"

  frontend_request {
    body_bytes     = 8192
    headers_to_log = []
    data_masking {
      query_params {
        mode  = "Hide"
        value = "*"
      }
    }
  }

  frontend_response {
    body_bytes     = 8192
    headers_to_log = []
  }

  backend_request {
    body_bytes     = 8192
    headers_to_log = []
    data_masking {
      query_params {
        mode  = "Hide"
        value = "*"
      }
    }
  }

  backend_response {
    body_bytes     = 8192
    headers_to_log = []
  }
}
