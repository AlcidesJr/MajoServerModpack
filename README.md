# MajoServerModpack

Mod independente, modular e extensível para Valheim.

## Estado

As fundações de arquitetura/governança (`MAJO-000`) estão concluídas. O primeiro core runtime (`MAJO-001`) está em verificação de uma correção documental final sobre a baseline de compatibilidade.

Próxima frente planejada, ainda não iniciada:

- `MAJO-002 — Authority and secure networking`

Ainda não há funcionalidades de gameplay do modpack. O núcleo atual fornece bootstrap, lifecycle, registries, diagnostics, metadata e adapters mínimos de plataforma sobre BepInEx + Jötunn.

## Plataforma

Baseline validada para build pela MAJO-001 e ainda candidata a suporte runtime:

- **Valheim** `1.0.15`;
- **BepInEx** `5.4.23.5` / BepInExPack_Valheim `5.4.2350`;
- **Jötunn** `2.30.1`;
- target do plugin: `net462`.

O smoke dentro do Valheim ainda não foi executado; portanto, esta combinação não é declarada suportada em runtime.

Nenhum mod funcional de terceiros é dependência do produto.

## Princípios

- mods externos são referências técnicas e funcionais;
- o código final do Majo é mantido e corrigido pelo próprio projeto;
- funcionalidades sobrepostas são consolidadas em uma implementação Majo;
- conflitos de regra, patch, persistência, rede, performance e input/UI são analisados antes de implementar;
- configurações de gameplay/world/server são server-authoritative;
- preferências puramente visuais e de input podem permanecer locais;
- hot paths são medidos antes/depois de otimizações materiais;
- GitHub e arquivos versionados são a fonte de verdade.

## Governança

O projeto segue o padrão operacional de [Fix_Development](https://github.com/AlcidesJr/Fix_Development), adaptado ao desenvolvimento de mods Valheim.

Consulte:

- `AGENTS.md`
- `docs/ARCHITECTURE.md`
- `docs/DEPENDENCIES.md`
- `docs/VERSIONING.md`
- `docs/CONFLICTS.md`
- `docs/SECURITY-BASELINE.md`
- `work/BOARD.md`
