# Teste Ília

## Pré-requisitos

- .NET 6 SDK
- Visual Studio 2022

## Como rodar

Essa aplicação pode ser executada diretamente no Visual Studio 2022 ou através dos comandos abaixo

```
cd /caminho/para/solucao
dotnet build
export ASPNETCORE_ENVIRONMENT=Development # opcional, Necessário para habilitar a Swagger UI
dotnet TesteIlia/bin/Debug/net6.0/TesteIlia.dll
```

Caso seja executado via VS, a aplicação atenderá requisições em https://localhost:7060. Caso seja executado via CLI,
atenderá requisições em https://localhost:5001.