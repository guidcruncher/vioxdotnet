# File: Makefile
IMAGE_NAME = guidcruncher/vioxdotnet-base

.PHONY: build publish release \

build:
	docker buildx build \
		--file ./Dockerfile \
		--tag docker.io/$(IMAGE_NAME):latest \
		--progress=plain \
		.

publish:
	docker buildx create --name vioxdotnet-base-builder --use --bootstrap
	-docker buildx build \
		--platform linux/arm64 \
		--builder vioxdotnet-base-builder \
		--file ./Dockerfile \
		--tag docker.io/$(IMAGE_NAME):dev \
		--tag ghcr.io/$(IMAGE_NAME):dev \
		--progress=plain \
		--push \
		.
	docker buildx rm -f vioxdotnet-base-builder

release:
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
