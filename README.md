# MajoServerModpack

Mod independente, modular e extensível para Valheim.

## Estado

A fundação arquitetural e de governança foi concluída pela `MAJO-000`.

Próxima frente planejada:

- `MAJO-001 — Core runtime`

Ainda não há implementação funcional de gameplay. A próxima tarefa deve construir somente o núcleo de runtime definido no roadmap, sem antecipar módulos posteriores.

## Plataforma

Dependências de plataforma aprovadas:

- **BepInEx** — loader/framework base e infraestrutura de patching;
- **Jötunn** — framework específico de Valheim, consumido por uma camada de adaptação própria.

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
