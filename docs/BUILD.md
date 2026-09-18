# Build — MajoServerModpack

## Toolchain

- Plugin target: .NET Framework 4.6.2 (`net462`).
- Linguagem: C# 8.
- Jötunn: `2.30.1`, fixado por PackageReference.
- BepInEx runtime baseline: `5.4.23.5`.
- BepInExPack_Valheim baseline correspondente: `5.4.2350`.
- Valheim baseline candidata: `1.0.15`.
- `Microsoft.NETFramework.ReferenceAssemblies.net462 1.0.3` é dependência somente de build para permitir compilação reproduzível também fora de Windows.

## Assemblies do jogo

DLLs do Valheim não são versionadas no repositório.

O build usa o mecanismo oficial do Jötunn para publicizar/referenciar assemblies a partir de uma instalação existente do Valheim. As propriedades relevantes são:

- `VALHEIM_INSTALL`;
- `VALHEIM_MANAGED`;
- `BEPINEX_PATH`;
- `ExecutePrebuild=true`.

O Jötunn 2.30.1 reconhece tanto `Valheim_Data/Managed` quanto `valheim_server_Data/Managed`.

## Build local

Pré-requisitos:

1. Valheim ou Valheim Dedicated Server instalado;
2. BepInEx 5.4.23.5 disponível;
3. .NET SDK com suporte a MSBuild SDK-style.

Exemplo:

```bash
dotnet build src/MajoServerModpack/MajoServerModpack.csproj \
  -c Release \
  -p:ExecutePrebuild=true \
  -p:VALHEIM_INSTALL="/caminho/Valheim" \
  -p:BEPINEX_PATH="/caminho/Valheim/BepInEx" \
  -p:VALHEIM_MANAGED="/caminho/Valheim/Valheim_Data/Managed"
```

Artefato próprio:

```text
src/MajoServerModpack/bin/Release/net462/MajoServerModpack.dll
```

Jötunn e BepInEx continuam frameworks externos e não são incorporados à DLL do Majo.

## Testes puros

```bash
dotnet run --project tests/MajoServerModpack.Core.Tests/MajoServerModpack.Core.Tests.csproj -c Release
```

O harness compila diretamente os fontes puros de `Core/`, sem Unity, BepInEx ou Jötunn. Ele não tenta simular o runtime Valheim.

## CI

O CI:

1. preserva o gate de governance;
2. executa os testes do Core;
3. instala temporariamente o Valheim Dedicated Server via SteamCMD;
4. baixa BepInEx 5.4.23.5 do release oficial e valida SHA-256;
5. executa o prebuild oficial do Jötunn;
6. compila `MajoServerModpack.dll`;
7. cria uma staging area contendo somente o artefato Majo e valida que DLLs do jogo/framework não foram incluídas.

Steam/Valheim/BepInEx/Jötunn são dependências de build/runtime externas; o Majo não faz download ou update delas quando carregado no jogo.

## Smoke runtime

Carregar a DLL em Valheim/BepInEx/Jötunn é um gate de integração real. Quando executado, validar no log:

- bootstrap Majo;
- versões detectadas;
- contexto de execução;
- módulos/inputs/patch surfaces;
- shutdown sem exceção.

Build bem-sucedido não deve ser descrito como prova de smoke runtime.
