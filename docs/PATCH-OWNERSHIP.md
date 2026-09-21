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

## Ownership

A MAJO-002 materializa o primeiro owner de runtime: `Core.SecureRpcGateway`.

Cada linha possui **um único owner**. Módulos adicionais aparecem somente como consumidores. Superfícies ainda não implementadas permanecem planejadas.

| Superfície Valheim | Owner Majo planejado | Consumidores | Referências que indicam conflito |
| --- | --- | --- | --- |
| ZNet.OnNewConnection | Core.SecureRpcGateway | Core.Network | Jötunn ModCompatibility |
| ZNet.RPC_ClientHandshake | Core.SecureRpcGateway | Core.Network | Jötunn ModCompatibility |
| ZNet.RPC_PeerInfo | Core.SecureRpcGateway | Core.Network, Compatibility | Jötunn ModCompatibility |
| ZNet.Disconnect(ZNetPeer) | Core.SecureRpcGateway | Core.Network | Valheim/Jötunn lifecycle |
| FejdStartup.ShowConnectError | Core.SecureRpcGateway | Compatibility UX | Jötunn ModCompatibility |
| ZRoutedRpc receive/routing | Core.SecureRpcGateway (reservado; sem patch na MAJO-002) | Administration, Config, Network | AdminPanel, FGN, Jötunn transport |
| ZDO send/streaming | Network.ZdoTransport | Network, WebMap diagnostics | FGN, VPO |
| ZDO ownership/handoff | Network.Ownership | Creatures, Ships, Performance | FGN, VPO |
| ZNetScene object streaming | Performance.WorldStreaming | Performance | VPO, FGN |
| WearNTear support/update | Performance.StructuralIntegrity | Building, Performance | VPO, ValheimPlus, FGN |
| CraftingStation extensions | Compatibility.CraftingStation | Crafting | StationExtensionGuard, WorkbenchesPlus |
| Inventory topology/save | Inventory.Topology | Equipment, QuickSlots, Tombstone | ExtraSlots, ValheimPlus |
| InventoryGui crafting list | Crafting.UI | Workbench UI | WorkbenchesPlus, ValheimPlus |
| Piece placement/building | Building.Placement | Plan, Blueprint, Administration | PlanBuild, ValheimPlus |
| Minimap/map state | Map.Core | SharedMap, Portals, WebMap snapshots | V+, TheGreatestMap, Portal |
| Player guardian powers/status effects | Powers.Core | Gameplay, UI | PassivePowers, ValheimPlus |
| Zone/world simulation | Network.WorldSimulation | Raids, Spawn, WebMap snapshot | FGN, V+ |
| Container discovery/indexing | Core.WorldIndex | Storage, Crafting, Building, Administration | GrabMaterials, V+, GearAndStorage |
| Container inventory transfer/access policy | Storage.ContainerTransactions | Crafting, Building, Grab, Administration | GrabMaterials, V+, GearAndStorage |

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


## MAJO-002 — superfícies implementadas

| Superfície | Frequência | Propósito | Risco | Fallback | Custo esperado |
| --- | --- | --- | --- | --- | --- |
| ZNet.OnNewConnection | por conexão | vincular RPC direto ao `ZNetPeer/ZRpc` real | assinatura/lifecycle mudar | falhar observavelmente; não criar sessão | O(1), 1 registro + mapping |
| ZNet.RPC_ClientHandshake | por conexão | enviar hello Majo | ordem do handshake mudar | conexão não se torna compatível | O(1), pacote pequeno |
| ZNet.RPC_PeerInfo | por conexão | gate fail-closed de compatibilidade | falso negativo de handshake | rejeitar conexão, nunca permitir silenciosamente | O(1) lookup |
| ZNet.Disconnect(ZNetPeer) | por disconnect | limpar sessão/rate/replay/mapping | overload/signature mudar | cleanup também ocorre no dispose do plugin | O(1) + cleanup das quotas da conexão |
| FejdStartup.ShowConnectError | somente erro de conexão | anexar motivo público Majo | UI mudar | log permanece disponível | O(1) |

A MAJO-002 **não** intercepta o hot path de `ZRoutedRpc`. O transporte privilegiado atual usa RPC direto registrado no `ZRpc` do peer para preservar o vínculo com a conexão real. Se uma necessidade futura exigir `ZRoutedRpc`, o owner continua sendo `Core.SecureRpcGateway`.
