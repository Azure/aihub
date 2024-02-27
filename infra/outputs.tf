output "resource_group_name" {
    value = "${azurerm_resource_group.rg.name}"
    description = "The name of the resource group"
}

output "subscription_id" {
    value = "${data.azurerm_subscription.current.subscription_id}"
    description = "The subscription ID used"
}

output "tenant_id" {
    value = "${data.azurerm_subscription.current.tenant_id}"
    description = "The tenant ID used"
}

output "storage_account_name" {
    value = "${module.st.storage_account_name}"
    description = "The name of the storage account"
}

output "storage_container_name" {
    value = "${module.st.storage_container_name}"
    description = "The name of the storage account"
}

output "search_service_name" {
    value = "${module.search.search_service_name}"
    description = "The name of the search service"
}

output "search_service_index_name" {
    value = "${module.search.search_index_name}"
    description = "The name of the search service index"
}

output "openai_service_name" {
    value = "${module.openai.openai_service_name}"
    description = "The name of the openai service"
}

output "embedding_deployment_name" {
    value = "${module.openai.embedding_deployment_name}"
    description = "The name of the embedding deployment"
}