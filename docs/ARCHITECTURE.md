# Arquitetura

## Objetivo

Construir um único produto Majo independente de mods funcionais de terceiros, com capacidade de crescer continuamente sem espalhar acoplamento a frameworks ou duplicar mecanismos internos.

## Camadas

```text
Valheim
│
├── BepInEx / Harmony
│
├── Jötunn
│
└── MajoServerModpack.dll
    │
    ├── Majo.Platform
    │   ├── Framework/
    │   ├── Game/
    │   ├── Content/
    │   ├── Network/
    │   └── Interop/
    │
    ├── Majo.Core
    │   ├── ModuleRegistry
    │   ├── ConfigRegistry
    │   ├── PatchCoordinator
    │   ├── SecureRpcGateway
    │   ├── Authority
    │   ├── Permissions
    │   ├── Persistence
    │   ├── WorldIndex
    │   ├── Scheduler
    │   ├── Cache
    │   ├── Diagnostics
    │   └── Compatibility
    │
    ├── Majo.UI
    │   └── MajoControlPanel
    │
    └── Majo.Modules
        ├── Player
        ├── Inventory
        ├── Equipment
        ├── Storage
        ├── Crafting
        ├── Building
        ├── Gameplay
        ├── Powers
        ├── Skills
        ├── Creatures
        ├── Map
        ├── Network
        ├── Performance
        ├── WebMap
        └── Administration
```

## BepInEx

Responsável por loader/plugin lifecycle, logging/config base e infraestrutura de patching. Não deve receber lógica de produto.

## Jötunn

Framework específico de Valheim. Deve ser usado onde elimina plumbing sensível a versões do jogo, por exemplo lifecycle de prefabs, items, pieces, localization, assets e integrações equivalentes.

Regra: usar Jötunn por conveniência arquitetural, não por reflexo. Hot paths e segurança podem exigir integração direta controlada com BepInEx/Harmony/Valheim.

## Majo.Platform

Anti-corruption layer entre módulos Majo e frameworks/jogo.

Não criar wrappers vazios para cada API. Encapsular apenas integrações cujo desacoplamento reduz custo de manutenção, risco de versão ou complexidade de testes.

## Majo.Core

Contém mecanismos comuns. Módulos não devem recriar:

- autenticação/autorização;
- config sync;
- patch ownership;
- scans globais;
- persistência base;
- scheduler;
- diagnostics;
- compatibility handshake.

## Autoridade

Categorias mínimas:

- `ClientPreference`: UI, hotkeys, layout e apresentação sem efeito de gameplay;
- `ServerAuthority`: valores que alteram gameplay, mundo, rede, persistência ou regras;
- `ServerPolicy`: servidor define se preferência normalmente local pode ser permitida, limitada ou forçada.

Single-player/host usa o mesmo pipeline, acumulando papéis de cliente, servidor e admin.

## Networking e segurança

Transporte pode usar abstrações Jötunn. Autoridade permanece Majo.

```text
Transport/RPC
    ↓
SecureRpcGateway
    ↓
Peer identity
    ↓
Authorization
    ↓
Payload validation
    ↓
Rate limiting
    ↓
Audit
    ↓
Server execution
```

## Persistência

Dados próprios usam namespace Majo e schemas versionados.

Versões independentes previstas:

- `MajoVersion`
- `ProtocolVersion`
- `ConfigSchema`
- `DataSchema`

## Performance

Serviços compartilhados devem impedir trabalho duplicado. Exemplo: um `WorldIndex` pode alimentar Storage, WebMap, Admin e Diagnostics sem quatro scans independentes.

## Distribuição

Meta arquitetural: uma DLL funcional principal do Majo. BepInEx/Jötunn permanecem dependências de plataforma externas.
