# Projetos de referência

Projetos desta lista são **referências**, não dependências funcionais.

Antes de implementar uma função inspirada neles, registrar comportamento desejado, overlap, patches/hot paths, rede, persistência, segurança e desenho Majo.

| Projeto | Uso primário como referência |
| --- | --- |
| Grantapher/ValheimPlus | catálogo amplo de gameplay/QoL/configurações |
| Valheim-Modding/Jotunn | framework de plataforma aprovado |
| fire-VA/FiresGhettoNetworking | networking, ZDO, queues, simulation |
| shudnal/ExtraSlots | slots, inventário, recovery, sync policy |
| morda0511/WorkbenchesPlus | crafting UI, sort/filter, dismantle |
| AdvizeGH/Advize_ValheimMods | funções selecionadas a catalogar |
| sirskunkalot/PlanBuild | planning, blueprints, build workflows |
| blaxxun-boop/PassivePowers | powers passivos |
| Turbero/valheim-DetailedLevels | skills/HUD/progresso |
| virtuaCode/valheim-mods | wheels, trash, emotes e QoL selecionado |
| ontrigger/ValheimPerformanceOptimizations | otimizações de rendering/logic/hot paths |
| gbahns/ValheimMods | storage, map, portal, pause, diagnostics |
| f00d4tehg0dz/valheim-webmap | mapa web 2D/3D, renderer e world sweep |
| TayrusCz/GearAndStorage | comportamento/UX de gear e storage |
| hldblc/ValheimAdminPanel | painel administrativo, segurança e UX |

## Regra de independência

Após uma feature entrar no Majo:

```text
bug Majo → issue Majo → task MAJO-xxx → fix Majo
```

Não depender da correção upstream para manter o produto operacional.

## Proveniência

Quando houver adaptação efetiva de código, registrar origem e atribuição/licença aplicável em documentação própria. Referência conceitual não deve ser confundida com derivação de código.

## Atualizações upstream

Upstream pode continuar sendo acompanhado por inteligência técnica. Novos fixes e soluções são avaliados por diff/changelog e, quando úteis, originam tarefa Majo própria.
