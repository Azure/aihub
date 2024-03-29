resource "azurerm_cognitive_account" "openai" {
  name                          = var.azopenai_name
  kind                          = "OpenAI"
  sku_name                      = "S0"
  location                      = var.location
  resource_group_name           = var.resource_group_name
  public_network_access_enabled = true
  custom_subdomain_name         = var.azopenai_name
}

resource "azurerm_cognitive_deployment" "gpt_35_turbo" {
  name                 = "gpt-35-turbo"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "gpt-35-turbo"
    version = "0613"
  }

  scale {
    type     = "Standard"
    capacity = 40
  }
}

resource "azurerm_cognitive_deployment" "embedding" {
  name                 = "text-embedding-ada-002"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "text-embedding-ada-002"
    version = "2"
  }

  scale {
    type     = "Standard"
    capacity = 40
  }
}

resource "azurerm_cognitive_deployment" "gpt_4" {
  name                 = "gpt-4"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "gpt-4"
    version = "1106-Preview"
  }

  scale {
    type     = "Standard"
    capacity = 20
  }
}

resource "azurerm_cognitive_deployment" "gpt4_vision" {
  name                 = "gpt4-vision"
  cognitive_account_id = azurerm_cognitive_account.openai.id
  rai_policy_name      = "Microsoft.Default"
  model {
    format  = "OpenAI"
    name    = "gpt-4"
    version = "vision-preview"
  }

  scale {
    type     = "Standard"
    capacity = 20
  }
}

resource "azurerm_role_assignment" "openai_user" {
  scope                = azurerm_cognitive_account.openai.id
  role_definition_name = "Cognitive Services OpenAI User"
  principal_id         = var.principal_id
}

