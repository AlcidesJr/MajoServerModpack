# MajoServerModpack

Mod independente, modular e extensível para Valheim.

## Estado

As fundações de arquitetura/governança (`MAJO-000`) e o primeiro core runtime (`MAJO-001`) estão concluídos.

A frente atual é:

- `MAJO-002 — Authority and secure networking` — implementação/verificação em andamento nesta branch.

Ainda não há funcionalidades de gameplay do modpack. A MAJO-002 adiciona somente infraestrutura de protocolo v1, handshake, identidade vinculada à conexão real, autorização, validação, rate limiting, replay protection, audit e failure isolation sobre BepInEx + Jötunn/Valheim.

## Plataforma

Baseline promovida pela MAJO-001 e reutilizada pela MAJO-002; suporte runtime da nova camada de rede ainda depende dos gates desta tarefa:

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
