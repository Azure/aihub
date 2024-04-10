variable "sp_name" {}
variable "redirect_uris" {
  type = list(string)
  default = []
}