# Versionamento

## Formato

O Majo usa exclusivamente três inteiros:

`MAJOR.MINOR.PATCH`

Sequência esperada:

`0.0.0 → 0.0.1 → 0.0.2 → ... → 0.0.10 → ... → 0.1.0 → 0.1.1 → ... → 1.0.0 → 1.0.1`

Sem sufixos `alpha`, `beta` ou `rc`, salvo decisão futura explícita.

## Intenção dos campos

### 0.0.x

Fundação e desenvolvimento inicial. Contratos ainda podem mudar de forma incompatível quando necessário e documentado.

### 0.x.y

Produto funcional em evolução.

- incremento de `MINOR`: marco funcional relevante;
- incremento de `PATCH`: melhoria/correção compatível dentro do marco.

### 1.0.0

Somente quando os contratos principais estiverem maduros: core, configuração, networking, persistência/migrations, segurança administrativa, compatibilidade e módulos definidos para o primeiro release estável.

### >= 1.0.0

Aplicar SemVer tradicional:

- PATCH: correção compatível;
- MINOR: funcionalidade compatível;
- MAJOR: quebra deliberada de compatibilidade.

## Versões internas independentes

A versão pública do mod não substitui:

- `ProtocolVersion` — compatibilidade de mensagens/RPC;
- `ConfigSchema` — formato/semântica das configurações;
- `DataSchema` — persistência/saves próprios;
- `CapabilityVersion` — somente se negociação de capacidades for introduzida no futuro;
- `LauncherBuild`/metadata — identificação diagnóstica do launcher, sem criar uma segunda versão pública por padrão.

Não aumentar protocolo/schema sem necessidade.

## Release 0.0.0

`0.0.0` representa a fundação inicial do repositório/arquitetura, não uma declaração de estabilidade.


## Launcher

No modelo inicial, runtime e launcher pertencem ao mesmo `MajoVersion` público.

Exemplo:

```text
MajoVersion: 0.4.0
Runtime: 0.4.0
Launcher: 0.4.0
ConfigSchema: 3
```

O launcher deve recusar edição destrutiva quando não compreender o `ConfigSchema`, apresentando diagnóstico em vez de reescrever configurações desconhecidas.
