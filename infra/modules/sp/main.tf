data "azurerm_client_config" "current" {}

resource "random_uuid" "uuid" {
}

resource "azuread_application" "sp" {
  display_name    = var.sp_name
  identifier_uris = ["api://${var.sp_name}"]
  owners = [
    data.azurerm_client_config.current.object_id
  ]

  web {
    implicit_grant {
      id_token_issuance_enabled = true
    }
    redirect_uris = []
  }

  api {
    mapped_claims_enabled          = true
    requested_access_token_version = null

    oauth2_permission_scope {
      admin_consent_description  = "Allow the application to access example on behalf of the signed-in user."
      admin_consent_display_name = "Allow the application to access example on behalf of the signed-in user."
      enabled                    = true
      id                         = random_uuid.uuid.result
      type                       = "User"
      user_consent_description   = "Allow the application to access example on your behalf."
      user_consent_display_name  = "Allow the application to access example on behalf of the signed-in user."
      value                      = "user_impersonation"
    }
  }
}

resource "azuread_service_principal" "sp" {
  client_id = azuread_application.sp.client_id
  owners = [
    data.azurerm_client_config.current.object_id
  ]
}

resource "azuread_service_principal_password" "sp" {
  service_principal_id = azuread_service_principal.sp.id
  end_date             = "2099-01-01T00:00:00Z"
}
