terraform {
  required_version = ">= 1.4.6"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.87.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "1.13.1"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "2.48.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "3.6.1"
    }
  }
}

provider "azurerm" {
  skip_provider_registration = false
  features {
    key_vault {
      purge_soft_delete_on_destroy = true
    }
    cognitive_account {
      purge_soft_delete_on_destroy = true
    }
    api_management {
      purge_soft_delete_on_destroy = true
    }
    resource_group {
      # This flag is set to mitigate an open bug in Terraform. For instance, the Resource Group is not deleted when a `Failure Anomalies` resource is present.
      # As soon as this is fixed, we should remove this.
      # Reference: https://github.com/hashicorp/terraform-provider-azurerm/issues/18026
      prevent_deletion_if_contains_resources = false
    }
  }
}

provider "azuread" {}
