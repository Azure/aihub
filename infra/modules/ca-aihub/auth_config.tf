locals {
  redirect_fqdn = jsondecode(azapi_resource.ca_back.output).properties.configuration.ingress.fqdn
}

module "sp" {
  count   = var.enable_entra_id_authentication ? 1 : 0
  source  = "../sp"
  sp_name = var.ca_name
}

resource "azapi_resource" "current" {
  count     = var.enable_entra_id_authentication ? 1 : 0
  type      = "Microsoft.App/containerApps/authConfigs@2023-05-01"
  name      = "Current"
  parent_id = azapi_resource.ca_back.id
  timeouts {}
  body = jsonencode({
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
  })
}

locals {
  fqdn                         = jsondecode(azapi_resource.ca_back.output).properties.configuration.ingress.fqdn
  update_redirect_uris_command = var.enable_entra_id_authentication ? "az ad app update --id ${module.sp[0].client_id} --web-redirect-uris https://${local.fqdn}/.auth/login/aad/callback" : ""
}

resource "null_resource" "update_redirect_uris" {
  count = var.enable_entra_id_authentication ? 1 : 0
  provisioner "local-exec" {
    command = local.update_redirect_uris_command
  }
  depends_on = [
    module.sp,
    azapi_resource.ca_back,
    azapi_resource.current
  ]
  triggers = {
    always_run = timestamp()
  }
}
