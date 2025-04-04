output "content_safety_endpoint" {
  value = azurerm_cognitive_account.content_safety.endpoint
}

output "content_safety_key" {
  value = azurerm_cognitive_account.content_safety.primary_access_key
}

output "cognitive_service_endpoint" {
  value = azurerm_cognitive_account.cognitive.endpoint
}

output "cognitive_service_key" {
  value = azurerm_cognitive_account.cognitive.primary_access_key
}

output "speech_key" {
  value = azurerm_cognitive_account.speech.primary_access_key
}

output "vision_endpoint" {
  value = azurerm_cognitive_account.vision.endpoint
}

output "vision_key" {
  value = azurerm_cognitive_account.vision.primary_access_key
}

output "form_recognizer_name" {
  value = azurerm_cognitive_account.form.name
}
