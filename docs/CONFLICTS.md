# Conflitos e sobreposições

Toda feature deve passar por análise de conflito antes de implementação.

## 1. Conflito funcional

Duas referências resolvem o mesmo problema.

Exemplo:

`CraftFromChest` + `GrabMaterials` + gestão de storage.

Ação: modelar um único serviço Majo em vez de portar múltiplos subsistemas.

## 2. Conflito de patches

Duas features precisam interceptar o mesmo método Valheim.

Ação:
- registrar owner no `PatchCoordinator`;
- preferir um ponto de patch Majo para hot paths;
- compor regras internamente.

## 3. Conflito de regras

Uma configuração altera implicitamente outra mecânica.

Exemplos:
- workbench/no-spawn/raids;
- slots/inventário/tombstone;
- server simulation/ownership/spawning.

Ação: configurações declaram `Requires`, `ConflictsWith` e `Affects`.

## 4. Conflito de persistência

Features escrevem em PlayerProfile, ZDO, world save ou custom data.

Ação:
- namespace Majo;
- ownership definido;
- schema versionado;
- migration;
- rollback/recovery quando dados puderem ser perdidos.

## 5. Conflito de performance

Múltiplos módulos repetem scans/cálculos.

Ação: consolidar em `WorldIndex`, caches, snapshots, schedulers e budgets compartilhados.

## 6. Conflito de rede

Features alteram ownership, streaming, RPC, compressão ou simulação.

Ação:
- ownership de patch;
- protocolo/versionamento;
- server authority;
- teste multiplayer;
- fallback/rollback.

## Níveis de resposta

- `BLOCK`: combinação inválida ou perigosa;
- `WARNING`: possível efeito colateral relevante;
- `INFO`: relação útil para o operador.

## Matriz inicial por domínio

| Domínio Majo | Referências principais | Sobreposição a analisar |
| --- | --- | --- |
| Network | FiresGhettoNetworking, VPO, ValheimPlus | ZDO send/streaming, ownership, simulation, queues |
| Performance | VPO, ValheimPlus, FGN | ZNetScene, WearNTear, physics, world scans |
| Inventory | ExtraSlots, ValheimPlus | schema, death/tombstone, UI, weight |
| Crafting | WorkbenchesPlus, ValheimPlus | crafting UI, multiplier, dismantle |
| Storage | GrabMaterials, GearAndStorage, ValheimPlus | pull/build/craft from chests, indexing |
| Building | PlanBuild, ValheimPlus | placement, support, blueprints, terrain |
| Powers | PassivePowers, ValheimPlus | effects, cooldowns, server rules |
| Skills/UI | DetailedLevels, ValheimPlus | HUD, messages, skill presentation |
| Map | TheGreatestMap, ValheimPlus, WebMap | map data, exploration, shared state |
| Admin | ValheimAdminPanel, ValheimPlus | privileged RPCs, world/player actions |
| WebMap | valheim-webmap | world sweep, ZDO access, 2D/3D budgets |
