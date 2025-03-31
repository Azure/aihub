resource "azurerm_virtual_network" "vnet" {
  name                = var.virtual_network_name
  address_space       = ["10.5.0.0/16"]
  location            = var.location
  resource_group_name = var.resource_group_name
}

resource "azurerm_subnet" "apim" {
  name                 = "snet-apim"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.5.0.0/27"] # A minimum subnet size of /26 or /27 is recommended when creating a new subnet for APIM. Reference: https://learn.microsoft.com/en-us/azure/api-management/integrate-vnet-outbound#prerequisites
  # Delegate the subnet to "Microsoft.Web/serverFarms". Reference: https://learn.microsoft.com/en-us/azure/api-management/integrate-vnet-outbound#delegate-the-subnet
  delegation {
    name = "apim-delegation"
    service_delegation {
      name = "Microsoft.Web/serverFarms"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/action",
      ]
    }
  }
}

resource "azurerm_subnet" "private_endpoints" {
  name                 = "snet-pe"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.5.1.0/28"] # Currently, this deployment creates around 9 services that requires private endpoints.
}

resource "azurerm_subnet" "cae" {
  name                 = "snet-cae"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.5.2.0/23"]

  # Delegate the subnet to "Microsoft.App/environments" cause of the use of workload profiles
  delegation {
    name = "cae-delegation"
    service_delegation {
      name = "Microsoft.App/environments"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}

