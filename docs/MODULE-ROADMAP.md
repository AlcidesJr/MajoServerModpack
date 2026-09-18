# Roadmap modular

Este roadmap define ordem arquitetural, não cronograma.

## Fundação

### MAJO-000 — Foundation, governance and architecture

Governança, arquitetura, referências, conflitos e contratos.

### MAJO-001 — Core runtime

Bootstrap BepInEx/Jötunn, `Majo.Platform`, lifecycle, module registry, logging, diagnostics básicos e patch registry vazio.

### MAJO-002 — Authority and secure networking

Handshake, versão/protocolo, identidade real de peer, SecureRpcGateway, permissions, rate limiting e audit base.

### MAJO-003 — Configuration platform

Config registry tipado, `ClientPreference`, `ServerAuthority`, `ServerPolicy`, validação, dependências/conflitos e sync.

### MAJO-004 — Majo Control Panel

UI gerada a partir de metadata de módulos/config, visão player/admin e indicadores de autoridade/risco/restart.

## Ordem de domínios recomendada após a fundação

1. **PlayerQoL/UI** — baixo risco e bom para validar framework/painel.
2. **Crafting UI** — WorkbenchesPlus como principal referência.
3. **Inventory/Equipment** — somente depois de migrations/recovery estarem definidas.
4. **Storage** — construir sobre WorldIndex e transferências atômicas.
5. **Gameplay/Powers/Skills** — usando config authority já madura.
6. **Building/Plan/Blueprint** — persistência e operações destrutivas.
7. **Map/Portals** — estado compartilhado e protocol/versioning.
8. **Network/Performance** — hot paths; baseline e benchmarks obrigatórios.
9. **Administration** — amplia SecureRpc já validado.
10. **WebMap 2D** — snapshot server-side e IO isolado.
11. **WebMap 3D** — opcional/experimental, com LOD/distance budgets.

## Regra para novas funções futuras

Uma feature nova não precisa caber em módulos existentes à força.

Fluxo:

```text
nova ideia
  ↓
identificar domínio e overlaps
  ↓
reutilizar serviço comum?
  ↓
sim → módulo existente
não → novo módulo com contrato explícito
```

A arquitetura deve continuar extensível sem transformar `Core` em depósito de lógica de gameplay.
