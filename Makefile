.PHONY: build, run

build:
	dotnet build

run:
	dotnet restore
	dotnet run
