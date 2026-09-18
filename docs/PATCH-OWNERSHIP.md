# Patch ownership e superfícies críticas

## Objetivo

Evitar que módulos Majo reproduzam o padrão de vários mods independentes aplicando Prefix/Postfix/Transpiler concorrentes sobre o mesmo hot path.

## Regra

Antes de criar um Harmony patch:

1. consultar este registro;
2. identificar se já existe owner;
3. se existir, integrar a nova regra ao owner ou justificar tecnicamente um segundo patch;
4. registrar ordem/semântica quando múltiplos patches forem inevitáveis;
5. adicionar regressão para interação entre features.

## Ownership inicial planejado

Ainda não há patches implementados. A tabela registra **domínios candidatos**, não classes finais.

| Superfície Valheim | Owner Majo planejado | Consumidores | Referências que indicam conflito |
| --- | --- | --- | --- |
| ZRoutedRpc receive/routing | Core.SecureRpc / Network | Admin, Config, Network | AdminPanel, FGN, Jötunn transport |
| ZDO send/streaming | Network.ZdoTransport | Network, WebMap diagnostics | FGN, VPO |
| ZDO ownership/handoff | Network.Ownership | Creatures, Ships, Performance | FGN, VPO |
| ZNetScene object streaming | Performance.WorldStreaming | Performance | VPO, FGN |
| WearNTear support/update | Performance.StructuralIntegrity | Building, Performance | VPO, ValheimPlus, FGN |
| CraftingStation extensions | Compatibility.CraftingStation | Crafting | StationExtensionGuard, WorkbenchesPlus |
| Inventory topology/save | Inventory.Topology | Equipment, QuickSlots, Tombstone | ExtraSlots, ValheimPlus |
| InventoryGui crafting list | Crafting.UI | Workbench UI | WorkbenchesPlus, ValheimPlus |
| Piece placement/building | Building.Placement | Plan/Blueprint/Admin | PlanBuild, ValheimPlus |
| Minimap/map state | Map.Core | SharedMap, Portals, WebMap snapshots | V+, TheGreatestMap, Portal |
| Player guardian powers/status effects | Powers.Core | Gameplay/UI | PassivePowers, ValheimPlus |
| Zone/world simulation | Network.WorldSimulation | Raids, Spawn, WebMap snapshot | FGN, V+ |
| Container discovery/access | Core.WorldIndex + Storage | Craft, Build, Grab, Admin | GrabMaterials, V+, GearAndStorage |

## Tipos de patch

Preferência:

- **Postfix/Prefix simples** quando a semântica é estável e mensurável;
- **Transpiler** somente quando não houver hook seguro mais simples;
- patch em hot path precisa registrar custo esperado;
- reflection scanning em loop não é aceitável sem cache;
- patches de compatibilidade devem fail-safe e logar de forma rate-limited.

## Patch budget

Cada patch futuro deve documentar:

- frequência aproximada;
- alocações esperadas;
- se roda client/server;
- estado lido/escrito;
- interação com outros owners;
- fallback em versão Valheim incompatível.

## Detecção de mudança de jogo

Quando uma atualização Valheim alterar assinatura/IL de uma superfície crítica:

- falhar de forma observável, não silenciosa, quando possível;
- desabilitar somente o módulo afetado se houver degradação segura;
- registrar capability/runtime check em `Majo.Platform`;
- nunca aplicar heurística destrutiva a save/persistência.
