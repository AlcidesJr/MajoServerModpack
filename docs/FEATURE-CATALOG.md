# Catálogo funcional de referências

## Objetivo

Este documento transforma os projetos de referência em requisitos e riscos analisáveis. Ele **não autoriza implementação automática**. Cada feature futura ainda precisa de tarefa própria, design Majo e validação contra a versão corrente do Valheim.

## Convenções

- **Local**: preferência de cliente sem efeito de gameplay compartilhado.
- **ServerAuthority**: servidor define o comportamento.
- **ServerPolicy**: servidor pode permitir/limitar uma preferência normalmente local.
- **Risk**: `Stable`, `Advanced` ou `Experimental` como intenção inicial; a classificação final pertence à tarefa da feature.
- **Destino**: módulo/serviço Majo provável, não API definitiva.

## ValheimPlus — catálogo-base amplo

ValheimPlus permanece a referência mais abrangente para descobrir superfícies configuráveis. As seções atuais encontradas no repositório são:

`AdvancedBuildingMode`, `AdvancedEditingMode`, `Armor`, `AutoStack`, `Bed`, `Beehive`, `Brightness`, `Building`, `Camera`, `Chat`, `CraftFromChest`, `Demister`, `Durability`, `Egg`, `EitrRefinery`, `EitrUsage`, `Experience`, `Fermenter`, `FireSource`, `FirstPerson`, `Food`, `FreePlacementRotation`, `Furnace`, `GameClock`, `Game`, `Gather`, `GridAlignment`, `HealthUsage`, `HotTub`, `Hotkeys`, `Hud`, `Inventory`, `Items`, `Kiln`, `LootDrop`, `Map`, `MonsterProjectile`, `Oven`, `Pickable`, `Player`, `PlayerProjectile`, `Procreation`, `SapCollector`, `Server`, `Shields`, `ShieldGenerator`, `Ship`, `Smelter`, `SpinningWheel`, `Stamina`, `StaminaUsage`, `StructuralIntegrity`, `Tameable`, `Time`, `Turret`, `Wagon`, `Ward`, `Windmill`, `WispSpawner` e `Workbench`.

### Regra de precedência

Quando uma feature do ValheimPlus se sobrepuser a uma referência especializada, a implementação Majo deve comparar ambas e preferir a solução especializada quando ela resolver melhor o problema sem criar regressão sistêmica.

Exemplos:

- CraftFromChest → comparar com GrabMaterials/GearAndStorage;
- crafting UI → WorkbenchesPlus;
- boss powers → PassivePowers;
- performance/network → VPO/FiresGhettoNetworking;
- inventory slots → ExtraSlots;
- admin → ValheimAdminPanel.

## Matriz funcional

| Referência | Feature/ideia | Destino Majo | Autoridade | Risco/Conflitos principais |
| --- | --- | --- | --- | --- |
| ValheimPlus | stamina, stamina usage, eitr/health usage | Gameplay/Player | ServerAuthority | sobreposição com powers/food |
| ValheimPlus | food duration/degradation | Gameplay/Food | ServerAuthority | buffs e mods de comida |
| ValheimPlus | carry weight, pickup, death penalty | Player | ServerAuthority | inventory/powers |
| ValheimPlus | HUD, clock, brightness, first person, camera | PlayerQoL/UI | Local/ServerPolicy | hotkeys/UI |
| ValheimPlus | gathering/pickables/loot | Gameplay/World | ServerAuthority | economia/balance |
| ValheimPlus | fire/smelter/kiln/furnace/oven/fermenter | Gameplay/Stations | ServerAuthority | Storage automation |
| ValheimPlus | map sharing/pins/position | Map | ServerAuthority + Local presentation | TheGreatestMap/WebMap |
| ValheimPlus | building/editing/rotation/grid | Building | ServerPolicy/Authority | PlanBuild, support |
| ValheimPlus | inventory/container sizing | Inventory/Storage | ServerAuthority | ExtraSlots/save/tombstone |
| ValheimPlus | craft from chest/autostack | Storage | ServerAuthority | GrabMaterials/GearAndStorage |
| ValheimPlus | structural integrity/workbench/ward | Building | ServerAuthority | WearNTear, spawn protection |
| ValheimPlus | tameable/procreation | Creatures | ServerAuthority | simulation/ownership |
| ValheimPlus | projectiles/shields/armor/durability | Combat | ServerAuthority | balance/hot paths |
| ValheimPlus | time/server/difficulty | Server/World | ServerAuthority | raids, spawn, pause |
| FiresGhettoNetworking | send rate/buffer/queue/ZDO rate | Network | ServerAuthority | sockets/ZDOMan |
| FiresGhettoNetworking | adaptive per-peer send rate | Network | ServerAuthority | feedback loops, fairness |
| FiresGhettoNetworking | negotiated compression | Network | ServerAuthority + capability | protocol compatibility |
| FiresGhettoNetworking | AOI RPC filtering | Network | ServerAuthority | routed RPC semantics |
| FiresGhettoNetworking | ZDO delta/distance throttling | Network | ServerAuthority | VPO/ZNetScene |
| FiresGhettoNetworking | server-side simulation/zones/raids | Network/World | ServerAuthority | Experimental; ownership/spawn |
| FiresGhettoNetworking | AI LOD/WearNTear skips | Performance | ServerAuthority | VPO/building |
| FiresGhettoNetworking | auto-tune/diagnostics | Diagnostics/Network | ServerPolicy | measurement stability |
| ExtraSlots | equipment/food/ammo/misc/utility/quick slots | Inventory/Equipment | ServerAuthority | save schema/tombstone |
| ExtraSlots | custom slot API concept | Inventory/Core | ServerAuthority | extensibility contract |
| ExtraSlots | slot progression/eligibility/weight/death rules | Inventory | ServerAuthority | persistence/gameplay |
| ExtraSlots | UI positions/hotkeys/colors/labels | UI | Local | no server sync by default |
| ExtraSlots | deferred recovery across topology changes | Persistence/Inventory | ServerAuthority | **must not lose items** |
| WorkbenchesPlus | recipe sort/filter/categories | Crafting/UI | Local | current crafting UI compatibility |
| WorkbenchesPlus | armor/material grouping | Crafting/UI | Local | modded item heuristics |
| WorkbenchesPlus | craft multiplier | Crafting | ServerAuthority | transaction/recipe semantics |
| WorkbenchesPlus | dismantle to materials | Crafting | ServerAuthority | dupes/refund policy |
| PlanBuild | planned pieces before resources | Building | ServerAuthority | persistence/support |
| PlanBuild | Plan Totem/resource aggregation | Building/Storage | ServerAuthority | storage index |
| PlanBuild | blueprint copy/save/share | Building/Persistence | ServerAuthority + Local files | validation/security |
| PlanBuild | cut/delete/bulk operations | Building/Admin-like | ServerAuthority | destructive actions |
| PlanBuild | terrain modification markers | Building/World | ServerAuthority | terrain persistence/perf |
| PassivePowers | passive boss powers | Powers | ServerAuthority | overlaps V+ guardian powers |
| PassivePowers | configurable active/passive effects | Powers | ServerAuthority | combat/stamina/wind |
| PassivePowers | boss kill requirements/max powers | Powers | ServerAuthority | progression |
| PassivePowers | activation spread/cooldown/depletion | Powers | ServerAuthority | RPC/status effects |
| DetailedLevels | dynamic skill progress HUD | Skills/UI | Local | low risk |
| DetailedLevels | sorting/decimal precision/messages | Skills/UI | Local | low risk |
| DetailedLevels | player stats in skills panel | Skills/UI | Local | UI lifecycle |
| VPO | ZNetScene streaming rewrite | Performance | Local/Server runtime | Network overlap |
| VPO | water/terrain burst jobs | Performance | Local/Server runtime | threading/Unity safety |
| VPO | threaded terrain collision | Performance | Local/Server runtime | Experimental |
| VPO | WearNTear support caching | Performance/Building | Runtime | structural integrity ownership |
| VPO | audio/light/particle culling | Performance | Local | rendering compatibility |
| VPO | reflection time slicing | Performance | Local | visual artifacts |
| VPO | ZDO ownership scan optimization | Performance/Network | Server runtime | FGN overlap |
| VPO | physics step cap | Performance | ServerPolicy | behavior tradeoff |
| WebMap | server-only 2D live map | WebMap | ServerAuthority | HTTP/filesystem/world scan |
| WebMap | fog/exploration approximation | WebMap/Map | ServerAuthority | privacy/spoilers |
| WebMap | pieces/vegetation/portal/player markers | WebMap | ServerAuthority | WorldIndex sharing |
| WebMap | 3D terrain/models/textures | WebMap3D | ServerAuthority | heavy export/render/load |
| WebMap | websocket/live player data | WebMap | ServerAuthority | privacy/auth/rate limits |
| WebMap | stats/events/export/webhooks | WebMap/Admin | ServerAuthority | data retention/external IO |
| ValheimAdminPanel | items/creatures/boss/world/player actions | Administration | Admin server authority | privileged RPC |
| ValheimAdminPanel | kick/ban/teleport/spawn/give | Administration | Admin server authority | identity/authz/audit |
| ValheimAdminPanel | diagnostics/server status | Administration/Diagnostics | Admin read-only | data collection |
| ValheimAdminPanel | backups/staged restore | Administration/Persistence | Admin | destructive/high risk |
| ValheimAdminPanel | roles/audit/guard/moderation | Administration | Admin | security-critical |
| ValheimAdminPanel | build/area/containers/raids/spawners tools | Administration | Admin | overlaps gameplay modules |
| gbahns GrabMaterials | pull nearby materials | Storage | ServerAuthority | atomic transfer |
| gbahns GrabMaterials | inventory panel/search/highlight | Storage/UI | Local + server data | container indexing |
| gbahns GrabMaterials | grab packs/delta/new-item flows | Storage/UI | Local policy + server transfer | inventory capacity |
| gbahns TheGreatestMap | shared exploration/markers/tombstones | Map | ServerAuthority | persistence/merge rules |
| gbahns TheGreatestMap | honest auto-recording | Map | ServerPolicy | anti-radar/privacy |
| gbahns TheGreatestPortal | directed portal destinations | Portals/Map | ServerAuthority | world persistence |
| gbahns TheGreatestPortal | open portal/map select/favorites/recents | Portals/UI | ServerAuthority + Local UI | protocol |
| gbahns PauseMyServer | unanimous pause | Server/World | ServerAuthority | time/physics/events |
| gbahns PauseMyServer | admin pause | Administration/Server | Admin | join/resume edge cases |
| gbahns DiagnoseServerLag | server/client tick/network/churn diagnosis | Diagnostics | ServerAuthority + Local | low-overhead telemetry |
| gbahns StationExtensionGuard | stale extension cleanup/diagnosis | Compatibility | Runtime | crafting station hot path |
| gbahns TheGreatestShips | ship variants/configurable stats | Ships/Content | ServerAuthority | prefab/content lifecycle |
| virtuaCode EmoteWheel | radial emote UI/gamepad | PlayerQoL/UI | Local | input conflicts |
| virtuaCode EquipWheel | radial item equip/use UI | Equipment/UI | Local | inventory/hotkeys |
| virtuaCode EquipWheel | shield auto-equip/filtering/multiple wheels | Equipment/UI | Local/ServerPolicy | overlaps V+ shield behavior |
| virtuaCode TrashItems | destroy items via UI/hotkey | Inventory/UI | Local action | irreversible; confirmation |
| Advize Armoire | equipment/wardrobe concept | Equipment/UI | TBD | inspect when tasked |
| Advize CartographySkill | cartography skill/progression | Skills/Map | ServerAuthority | TheGreatestMap |
| Advize ColorfulVines | cosmetic vine customization | Building/Visual | Local/ServerPolicy | prefab/material |
| Advize PlantEasily | planting QoL | Farming | ServerAuthority/Policy | placement/balance |
| Advize PlantEverything | expanded planting | Farming/World | ServerAuthority | prefab/world rules |
| Advize Spyglass | zoom/spyglass | PlayerQoL | Local | camera/input |
| Advize StumpsRegrow | stump/tree regrowth | World | ServerAuthority | world persistence |
| GearAndStorage | gear/storage behavior reference only | Equipment/Storage | TBD | public repo has no implementation |
| Jötunn | framework services, not feature source | Platform | N/A | framework boundary |

## Requisitos transversais extraídos das referências

### Inventory

1. Topologia versionada.
2. Nenhuma mudança de slots pode destruir item.
3. Recovery/deferred storage é obrigatório quando o novo layout não comportar tudo.
4. Tombstone/death/logout/reconnect precisam de casos de teste próprios.
5. UI local não deve alterar autoridade de slots.

### Storage

1. Transferência deve ser atômica: retirar do container somente após confirmar destino/capacidade.
2. Um índice compartilhado deve substituir scans repetidos de containers.
3. Craft/build/pull devem compartilhar a mesma semântica de acesso e distância.
4. Regras de ward/ownership precisam ser respeitadas.

### Network/Performance

1. FGN e VPO se sobrepõem em ZDO/streaming/ownership; não portar ambos separadamente.
2. Server-side simulation fica `Experimental` até provar estabilidade.
3. Mudanças em physics/terrain threading exigem benchmark e regressão visual/física.
4. Diagnóstico deve existir antes de auto-tuning agressivo.

### Building

1. PlanBuild implica dados próprios e operações destrutivas; blueprint é um subdomínio, não apenas UI.
2. Building/Storage precisam compartilhar resource resolution.
3. Structural integrity tem ownership único de patch.

### Map/WebMap

1. Map in-game e WebMap devem compartilhar snapshots/índices quando possível.
2. WebMap 3D deve ser opcional e possuir budgets explícitos.
3. Distância detalhada de objetos 3D deve ser limitada/configurável; alvo inicial a estudar: faixa curta próxima ao observador, com LOD/occlusion para o restante.
4. HTTP/WebSocket nunca deve manipular Unity mutable state diretamente fora da main thread.

### Administration

1. Toda ação privilegiada é request ao servidor.
2. Client-side admin UI é conveniência, nunca controle de segurança.
3. Identidade, autorização, payload, rate limit e audit precedem execução.
4. Operações destrutivas precisam de confirmação/rollback quando aplicável.

## Features explicitamente não autorizadas pela MAJO-000

Nenhuma linha desta matriz autoriza implementação. A partir da MAJO-001, cada conjunto deve virar tarefa atômica e provar compatibilidade com as demais decisões.
