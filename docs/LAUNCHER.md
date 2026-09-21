# Launcher

## Objetivo

O Majo terá uma aplicação desktop própria para preparar e iniciar a experiência modded sem transformar essa aplicação em requisito do runtime do Valheim.

```text
MajoLauncher
├── InstallationDiscovery
├── EnvironmentPreflight
├── CompatibilityValidation
├── ConfigurationEditor
├── Profiles
├── LaunchOrchestration
└── Diagnostics
```

## Princípio central

```text
Launcher = conveniência, configuração e orquestração
Runtime = regras, autoridade e execução do mod
Servidor = autoridade final de gameplay compartilhado
```

O launcher nunca é prova de identidade, admin ou permissão.

## Modos de uso

### Caminho preferencial

```text
MajoLauncher
  -> preflight
  -> configuração
  -> launch
  -> Steam/Valheim
  -> BepInEx
  -> Jötunn
  -> MajoServerModpack.dll
```

### Caminho manual suportado

```text
Steam/Valheim
  -> BepInEx
  -> Jötunn
  -> MajoServerModpack.dll
```

O segundo caminho deve continuar funcionando.

## Configuração

A MAJO-003 é pré-requisito porque o launcher não possuirá catálogo paralelo de settings.

O contrato deve representar, no mínimo:

```text
Key
Module
Type
Default
Min/Max
AllowedValues
Authority
RuntimeRequirement
Risk
Requires
ConflictsWith
Affects
Description
SchemaVersion
```

O launcher lê esse contrato e renderiza os controles correspondentes.

### Precedência

- `ClientPreference`: launcher pode editar localmente.
- `ServerAuthority` em singleplayer/host local: launcher pode preparar configuração do servidor local.
- `ServerAuthority` de servidor remoto: launcher não substitui o servidor.
- `ServerPolicy`: valor efetivo conectado é decidido pelo servidor.

## Perfis

Perfis guardam valores, não uma segunda definição de settings.

Exemplos futuros:

- padrão;
- performance;
- hardcore;
- servidor específico;
- teste/desenvolvimento.

## Preflight

O launcher deve validar, conforme contratos promovidos:

- caminho do Valheim;
- versão/build do Valheim;
- presença de BepInEx;
- presença de Jötunn;
- presença/versão do Majo;
- hashes quando exigidos;
- config/schema compreendido;
- arquivos ausentes ou duplicados;
- incompatibilidades conhecidas.

Classificação:

- ERROR — não iniciar modded com segurança;
- WARNING — pode iniciar, mas há risco conhecido;
- INFO — diagnóstico.

## Inicialização

A MAJO-004 deverá validar o mecanismo correto de launch para preservar Steam/platform identity e BepInEx.

Requisitos:

- launcher não precisa permanecer aberto;
- não injetar código próprio no processo do jogo;
- não contornar Steam de forma incompatível;
- argumentos de launch allowlisted/estruturados;
- paths normalizados e corretamente quoted;
- não executar strings arbitrárias como shell command.

Modo vanilla/safe pode ser estudado, mas não está automaticamente aprovado se exigir mutação insegura da instalação.

## Atualização/instalação

O launcher inicial não recebe autorização implícita para se tornar updater.

Se futuramente gerenciar instalação/update:

1. somente origens aprovadas;
2. manifest versionado;
3. hashes obrigatórios;
4. staging antes de substituir arquivos;
5. rollback;
6. nunca executar payload não validado;
7. update de framework permanece operação explicitamente promovida.

## Segurança

### Não confiável para o servidor

O usuário controla launcher, arquivos locais, argumentos e environment variables. Nenhuma dessas fontes pode conceder autoridade no servidor.

### Filesystem

Operar apenas sobre roots explicitamente descobertos/aprovados:

- diretório da própria aplicação;
- instalação Valheim selecionada;
- diretório de configuração Majo;
- profiles/logs do launcher.

Operações destrutivas futuras devem proteger contra path traversal e symlink/reparse surprises.

## Relação com tarefas anteriores

### MAJO-000

A decisão original de uma DLL funcional principal continua válida para o runtime in-game. A evolução é:

```text
produto Majo != somente DLL
runtime Majo dentro do Valheim = uma DLL funcional principal
```

MAJO-000 permanece DONE.

### MAJO-001

Nenhuma alteração de código é necessária. O bootstrap atual continua sendo a entrada do runtime. O launcher apenas inicia o jogo.

### MAJO-002

Continua responsável por identidade/autoridade/RPC in-game.

- launcher não participa do handshake como peer;
- não fornece admin status;
- não fornece peer identity confiável;
- não bypassa rate limiting/permissions;
- valor escrito pelo launcher continua não confiável como claim de autoridade.

### MAJO-003

Deve fornecer schema/config contract consumido pelo launcher.

```text
MAJO-003 Config Platform
        ↓
machine-readable schema/metadata
        ↓
MAJO-004 Launcher
        ↓
editor de valores
```

### MAJO-005

A UI in-game e o launcher consomem a mesma metadata de configuração, evitando dois catálogos.

## Critérios iniciais da MAJO-004

- launcher inicia sem Valheim em execução;
- descobre instalação de forma segura;
- preflight determinístico;
- edição preserva settings desconhecidos quando aplicável;
- escrita de config atômica/recuperável;
- launcher não é necessário depois do launch;
- jogo pode iniciar manualmente sem launcher;
- nenhuma autoridade de servidor depende do launcher;
- paths/args não permitem command injection;
- versão/schema incompatível falha com segurança.
