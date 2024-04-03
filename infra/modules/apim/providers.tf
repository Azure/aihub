terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.87.0"
    }
    azapi = {
      source  = "azure/azapi"
      version = "= 1.12.1"
    }
  }
}
