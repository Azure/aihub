module "sp" {
  count   = var.enable_entra_id_authentication ? 1 : 0
  source  = "../sp"
  sp_name = var.ca_name
  redirect_uris = [
    "https://${var.ca_name}.${var.cae_default_domain}/.auth/login/aad/callback"
  ]
}

resource "azapi_resource" "current" {
  count     = var.enable_entra_id_authentication ? 1 : 0
  type      = "Microsoft.App/containerApps/authConfigs@2023-05-01"
  name      = "Current"
  parent_id = azapi_resource.ca_back.id
  timeouts {}
  body = {
    properties = {
      platform = {
        enabled = true
      }
      globalValidation = {
        redirectToProvider          = "azureactivedirectory"
        unauthenticatedClientAction = "RedirectToLoginPage"
      }
      identityProviders = {
        azureActiveDirectory = {
          enabled           = true
          isAutoProvisioned = true
          registration = {
            clientId                = "${module.sp[0].client_id}"
            clientSecretSettingName = "microsoft-provider-authentication-secret"
            openIdIssuer            = "https://sts.windows.net/${var.tenant_id}/v2.0"
          }
          validation = {
            allowedAudiences = [
              "api://${module.sp[0].client_id}"
            ]
          }
        }
      }
      login = {
        preserveUrlFragmentsForLogins = false
      }
    }
  }
}
