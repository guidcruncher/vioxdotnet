SOLUTION = viox-plugin.sln


.PHONY: help build test restore deploy clean

default: help

help: ## Show command usage and description format deploy
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

test: ## Run unit tests
	dotnet test ./viox-plugins.sln --logger "console;verbosity=detailed"

restore: ## Restores packages and npm modules
	dotnet restore ./viox-plugins.sln

build: ## Build the plugins
	dotnet  build ./viox-plugins.sln

format: ## Formats the code
	dotnet format ./viox-plugins.sln

clean: ## Cleans the solution
	dotnet clean ./viox-plugins.sln

deploy: ## Deploys the Plugin
	@find . -type f -name "*.dll" -not -path "../../../runtime/plugins/*" -exec sudo cp -f {} "../../../runtime/plugins/" \;
	@find . -type f -name "*.dll" -not -path "../../../runtime/plugins/*" -exec sudo cp -f {} "../../../runtime/plugins/" \;
	@sudo rm  ../../../runtime/plugins/Viox.Core.*
