# MajoServerModpack

Mod independente, modular e extensível para Valheim.

## Estado

O projeto está em fundação arquitetural. Nenhuma funcionalidade de gameplay deve ser implementada antes do fechamento da `MAJO-000`.

## Plataforma

Dependências de plataforma aprovadas:

- **BepInEx** — loader/framework base e infraestrutura de patching;
- **Jötunn** — framework específico de Valheim, consumido por uma camada de adaptação própria.

Nenhum mod funcional de terceiros é dependência do produto.

## Princípios

- mods externos são referências técnicas e funcionais;
- o código final do Majo é mantido e corrigido pelo próprio projeto;
- funcionalidades sobrepostas são consolidadas em uma implementação Majo;
- conflitos de regra, patch, persistência, rede e performance são analisados antes de implementar;
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
- `work/BOARD.md`
