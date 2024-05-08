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
  default = "ghcr.io/azure/aihub/aihub-prepdocs:1.0.11"
}

variable "ca_plugin_image" {
  default = "ghcr.io/azure/aihub/aihub-plugin:1.0.11"
}

variable "ca_fsi_plugin_image" {
  default = "ghcr.io/azure/aihub/aihub-fsiplugin:1.0.11"
}

variable "ca_aihub_image" {
  default = "ghcr.io/azure/aihub/aihub:1.0.11"
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

variable "deploy_bing" {
  default = true
}

variable "pbi_report_link" {
  default = ""
}
