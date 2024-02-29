terraform {
  backend "azurerm" {
    resource_group_name  = "sshsstates"
    storage_account_name = "sshsstg01"
    container_name       = "sshsstatedevops01"
    key                  = "sshsstatedevops01.tfstate"
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.0.0"
    }
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

variable "docker_registry_password" {
  type        = string
  description = "Docker Registry password"
}
variable "docker_registry_url" {
  type        = string
  description = "Docker Registry URL"
}
variable "docker_registry_username" {
  type        = string
  description = "Docker Registry username"
}
variable "db_admin_username" {
  type        = string
  description = "SQL admin username"
}
variable "db_admin_password" {
  type        = string
  description = "SQL admin password"
}
variable "db_connection_string" {
  type        = string
  description = "SQL connection string"
}

resource "azurerm_resource_group" "rg" {
  name     = "sshs"
  location = "eastus"
}

resource "azurerm_key_vault" "keyvault" {
  name                        = "sshskeyvault01"
  resource_group_name         = azurerm_resource_group.rg.name
  location                    = azurerm_resource_group.rg.location
  enabled_for_disk_encryption = false
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  sku_name                    = "standard"
}

resource "azurerm_container_registry" "acr" {
  name                = "sshsconrgs01"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Basic"
  admin_enabled       = true
}

resource "azurerm_service_plan" "appservice-plan" {
  name                = "sshsappsrvpln01"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "F1"
}

resource "azurerm_linux_web_app" "appsrv-prodcatalog" {
  name                = "ogionsshsappsrvcat01"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.appservice-plan.id

  site_config {
  }

  app_settings = {
    "DOCKER_REGISTRY_SERVER_URL"          = var.docker_registry_url
    "DOCKER_REGISTRY_SERVER_USERNAME"     = var.docker_registry_username
    "DOCKER_REGISTRY_SERVER_PASSWORD"     = var.docker_registry_password
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_key_vault_access_policy" "keyvault-sshsappsrvcat01-accesspolicy" {
  key_vault_id = azurerm_key_vault.keyvault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id

  object_id = azurerm_linux_web_app.appsrv-prodcatalog.identity.0.principal_id

  secret_permissions = [
    "Get",
    "List",
  ]
}

resource "azurerm_key_vault_access_policy" "keyvault-sshsapp-accesspolicy" {
  key_vault_id = azurerm_key_vault.keyvault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id

  object_id = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete",
    "Recover",
    "Backup",
    "Restore",
    "Purge"
  ]
}

resource "azurerm_key_vault_secret" "secret_db_connectionstring" {
  name         = "ConnectionStrings--ProductCatalogDbPgSqlConnection"
  value        = var.db_connection_string
  key_vault_id = azurerm_key_vault.keyvault.id

  depends_on = [
    azurerm_key_vault_access_policy.keyvault-sshsapp-accesspolicy
  ]
}

resource "azurerm_postgresql_server" "pg-server" {
  name                         = "sshsdbsrvprodcatalog01"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  administrator_login          = var.db_admin_username
  administrator_login_password = var.db_admin_password
  sku_name                     = "B_Gen5_1"
  version                      = "11"
  ssl_enforcement_enabled      = true
  auto_grow_enabled            = false
  storage_mb                   = 5120
}
resource "azurerm_postgresql_database" "pg-prodcatalog" {
  name                = "sshsdbprodcatalog01"
  resource_group_name = azurerm_resource_group.rg.name
  charset             = "UTF8"
  collation           = "English_United States.1252"
  server_name         = azurerm_postgresql_server.pg-server.name
}
resource "azurerm_postgresql_firewall_rule" "azure-firwall-rule" {
  name                = "AzureFirewallRule"
  resource_group_name = azurerm_resource_group.rg.name
  server_name         = azurerm_postgresql_server.pg-server.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}