# dapr-demo
Dapr demo for academic purposes

## Requirements
1) [Docker](https://www.docker.com/)
2) [.NET SDK 6.0](https://dotnet.microsoft.com/en-us/download)

## How to run
To get the application running open a command prompt at the repository root and execute the following steps:
1) Run the **“docker-compose -f docker-compose-infra.yml up -d”** command to launch base infrastructure containers.
2) Run the **“docker-compose up -d --build”** command to build applications, build docker images and launch the application containers plus its Dapr sidecar containers.
