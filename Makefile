APP_NAME = Viox.Server
COMPOSE = docker compose
IMAGE_NAME = guidcruncher/vioxdotnet
SOLUTION = viox.sln
CONTAINER_NAME ?= viox-net


.PHONY: help build client clean-container preview up down restart logs status shell clean dotnet-build client-build publish release format debug dev update-packages

default: help

help: ## Show command usage and description
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

clean-container: ## Stop and remove container
	@echo "Checking status of container: $(CONTAINER_NAME)..."
	@if [ $$(docker ps -q -f name=^/$(CONTAINER_NAME)$$) ]; then \
		echo "Stopping running container..."; \
		docker stop $(CONTAINER_NAME); \
	fi
	@if [ $$(docker ps -aq -f name=^/$(CONTAINER_NAME)$$) ]; then \
		echo "Removing container..."; \
		docker rm $(CONTAINER_NAME); \
	else \
		echo "Container $(CONTAINER_NAME) does not exist."; \
	fi

update-packages: ## Performs a package update of all outdated packages.
	@for proj in $$(find . -name "*.csproj"); do \
		dotnet list "$$proj" package --outdated | awk '/^   >/ {print $$2}' | xargs -r -I % dotnet add "$$proj" package %; \
	done

build: ## Build the ARM64 Docker image using docker compose
	$(COMPOSE) build

up: ## Start the container service in detached mode
	$(COMPOSE) up -d

preview: ## Starts vite in Development mode
	$(COMPOSE) up --build --force-recreate -d
	cd ./src/AudioHub && npm run dev
	$(COMPOSE) down
	$(COMPOSE) rm -f

client: ## Run client on port 5173
	cd ./src/AudioHub && npm  run dev

dev: clean-container ## Run container in Development mode
	$(COMPOSE) up --build --force-recreate
	$(COMPOSE) rm -f

debug: clean-container ## Start the container in debug mode
	$(COMPOSE) up --build --force-recreate
	$(COMPOSE) rm -f

down: ## Stop and remove running container services
	$(COMPOSE) down
	$(COMPOSE) rm -f 

restart: ## Restart the container services
	$(COMPOSE) restart

logs: ## Tail container logs in real time
	$(COMPOSE) logs -f

status: ## Show container health and runtime status
	$(COMPOSE) ps

shell: ## Open an interactive bash shell in the container (for ALSA configuration)
	$(COMPOSE) exec -i -t viox-net bash

dotnet-build: ## Build the .NET 10 project locally
	dotnet build ./src/Viox.Server/Viox.Server.csproj -c Release

client-build: ## Build the Vue Client project
	cd ./src/AudioHub && npm run format && npm run build

clean: ## Stop services, remove volumes, and clean dangling docker images
	$(COMPOSE) down --volumes --remove-orphans
	docker image prune -f
	dotnet clean $(SOLUTION)
	rm -rf ./src/AudioHub/node_modules ./src/AudioHub/wwwroot
	rm -rf ./publish
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} +

publish: ## Publish Development image to Docker Repository
	docker buildx create --name vioxdotnet-base-builder --use --bootstrap
	-docker buildx build \
		--platform linux/arm64 \
		--builder vioxdotnet-base-builder \
		--file ./Dockerfile \
		--tag ghcr.io/$(IMAGE_NAME):dev \
		--tag docker.io/$(IMAGE_NAME):dev \
		--progress=plain \
		--push \
		.
	docker buildx rm -f vioxdotnet-base-builder

release: ## Publish Release image to Docker Repository
	docker buildx create --name vioxdotnet-base-builder --use --bootstrap
	-docker buildx build \
		--builder vioxdotnet-base-builder \
		--platform linux/arm64 \
		--file ./Dockerfile \
		--tag docker.io/$(IMAGE_NAME):latest \
		--tag docker.io/$(IMAGE_NAME):dev \
		--tag ghcr.io/$(IMAGE_NAME):latest \
		--tag ghcr.io/$(IMAGE_NAME):dev \
		--progress=plain \
		--push \
		.
	docker buildx rm -f vioxdotnet-base-builder

format: ## Format and organise code
	cd ./src/AudioHub && npm run format
	@for f in *.json; do \
		[ -f "$$f" ] || continue; \
		echo "$$f"; \
		tmp=$$(mktemp) && { jq '.' "$$f" > "$$tmp" && mv "$$tmp" "$$f" || rm -f "$$tmp"; }; \
	done
	dotnet format $(SOLUTION)
