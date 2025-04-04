variable "resource_group_name" {
  default = "rg-aihub"
}

variable "location" {
  default = "eastus2"
}

variable "location_azopenai" {
  default = "swedencentral"
}

variable "location_content_safety" {
  default = "eastus"
}

variable "log_name" {
  default = "log-aihub"
}

variable "azopenai_name" {
  default = "cog-openai-aihub"
}

variable "content_safety_name" {
  default = "cog-content-safety-aihub"
}

variable "cognitive_services_name" {
  default = "cog-cognitive-aihub"
}

variable "vision_name" {
  default = "cog-vision-aihub"
}

variable "speech_name" {
  default = "cog-speech-aihub"
}

variable "bing_name" {
  default = "cog-bing-aihub"
}

variable "search_name" {
  default = "srch-aihub"
}

variable "form_recognizer_name" {
  default = "cog-forms-aihub"
}

variable "storage_account_name" {
  default = "stgenai"
}

variable "apim_name" {
  default = "apim-aihub"
}

variable "appi_name" {
  default = "appi-aihub"
}

variable "publisher_name" {
  default = "contoso"
}

variable "publisher_email" {
  default = "admin@contoso.com"
}

variable "virtual_network_name" {
  default = "vnet-aihub"
}

variable "managed_identity_name" {
  default = "id-aihub"
}

variable "cae_name" {
  default = "cae-aihub"
}

variable "ca_chat_name" {
  default = "ca-chat-aihub"
}

variable "ca_prep_docs_name" {
  default = "ca-prep-docs-aihub"
}

variable "ca_aihub_name" {
  default = "ca-aihub"
}

variable "cv_name" {
  default = "cv-aihub"
}

variable "ca_chat_image" {
  default = "ghcr.io/azure/activate-genai/aihub-chat:1.0.0-preview.0"
}

variable "ca_prep_docs_image" {
  default = "ghcr.io/azure/aihub/aihub-prepdocs:1.0.15"
}

variable "ca_plugin_image" {
  default = "ghcr.io/azure/aihub/aihub-plugin:1.0.15"
}

variable "ca_aihub_image" {
  default = "ghcr.io/azure/aihub/aihub:1.0.15"
}

variable "ai_services_name" {
  default = "ai-services"
}

variable "ai_foundry_name" {
  default = "hub"
}

variable "ai_foundry_project_name" {
  default = "aiprj"
}

variable "ai_foundry_kv_name" {
  default = "kv-hub"
}

variable "ai_foundry_st_name" {
  default = "sthub"
}

variable "bing_account_name" {
  default = "bing"
}

variable "use_random_suffix" {
  default = true
}

variable "enable_entra_id_authentication" {
  default = true
}

variable "enable_apim" {
  default = true
}

variable "pbi_report_link" {
  default = ""
}

variable "enable_openai_plugin_call_transcript" {
  default = false
}

variable "enable_openai_plugin_compare_financial_products" {
  default = false
}

variable "use_private_endpoints" {
  type    = bool
  default = false
}

variable "allowed_ips" {
  type    = list(string)
  default = []
}
