variable "resource_group_name" {
  default = "rg-activate-genai"
}

variable "location" {
  default = "westeurope"
}

variable "location_azopenai" {
  default = "swedencentral"
}

variable "log_name" {
  default = "log-activate-genai"
}

variable "azopenai_name" {
  default = "cog-openai-activate-genai"
}

variable "content_safety_name" {
  default = "cog-content-safety-activate-genai"
}

variable "cognitive_services_name" {
  default = "cog-cognitive-activate-genai"
}

variable "speech_name" {
  default = "cog-speech-activate-genai"
}

variable "bing_name" {
  default = "cog-bing-activate-genai"
}

variable "search_name" {
  default = "srch-activate-genai"
}

variable "form_recognizer_name" {
  default = "cog-forms-activate-genai"
}

variable "storage_account_name" {
  default = "stgenai"
}

variable "apim_name" {
  default = "apim-activate-genai"
}

variable "appi_name" {
  default = "appi-activate-genai"
}

variable "publisher_name" {
  default = "contoso"
}

variable "publisher_email" {
  default = "admin@contoso.com"
}

variable "virtual_network_name" {
  default = "vnet-activate-genai"
}

variable "managed_identity_name" {
  default = "id-activate-genai"
}

variable "cae_name" {
  default = "cae-activate-genai"
}

variable "ca_chat_name" {
  default = "ca-chat-activate-genai"
}

variable "ca_prep_docs_name" {
  default = "ca-prep-docs-activate-genai"
}

variable "ca_aihub_name" {
  default = "ca-aihub-activate-genai"
}

variable "ca_chat_image" {
  default = "ghcr.io/azure/activate-genai/aihub-chat:1.0.0-preview.0"
}

variable "ca_prep_docs_image" {
  default = "ghcr.io/azure/aihub/aihub-prepdocs:1.0.1"
}

variable "ca_aihub_image" {
  default = "ghcr.io/azure/aihub/aihub:1.0.2-preview.23"
}

variable "use_random_suffix" {
  default = true
}

variable "enable_entra_id_authentication" {
  default = true
}

variable "enable_apim" {
  default = false
}

variable "deploy_bing" {
  default = false
}

variable "bing_key" {
  default = "<replace with a valid bing key>"
}

variable "pbi_report_link" {
  default = ""
}
