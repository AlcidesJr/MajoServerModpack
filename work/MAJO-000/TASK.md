# TASK — MAJO-000

## Título

Foundation, governance and architecture

## Objetivo

Criar uma fundação auditável para o MajoServerModpack antes de qualquer implementação funcional.

## Contexto

O projeto pretende consolidar, reimplementar e evoluir funcionalidades inspiradas em múltiplos mods Valheim, permanecendo independente deles e preparado para expansão futura.

## Escopo

- adotar governança do Fix_Development;
- registrar arquitetura;
- registrar dependências de plataforma;
- registrar política de referências;
- registrar versionamento;
- registrar análise obrigatória de conflitos;
- criar Board e próximos marcos;
- estabelecer princípios de autoridade, segurança, patch ownership, input ownership, compatibilidade e performance.

## Fora de escopo

- implementar módulos de gameplay;
- adicionar código funcional de referências;
- produzir release jogável;
- implementar SecureRpc, config sync, painel, WebMap ou otimizações.

## Critérios de aceite

- [x] Arquitetura em camadas documentada.
- [x] BepInEx + Jötunn classificados como frameworks de plataforma.
- [x] Mods funcionais classificados como referências.
- [x] Versionamento de três inteiros documentado.
- [x] Conflitos funcionais/técnicos formalizados.
- [x] Autoridade e princípio de segurança RPC registrados.
- [x] Board inicial criado.
- [x] Compatibilidade por lado/capability e ownership de input documentados.
- [x] Catálogo funcional das referências revisado em profundidade e vinculado à matriz de conflitos.
- [ ] Review independente concluído.
- [x] Security review da fundação concluído — PASS; hardening registrado em `docs/SECURITY-BASELINE.md`.
- [ ] PR integrado e closeout registrado.

## Dependências

- DEPENDS_ON: none

## Riscos conhecidos

- projetar abstrações prematuramente sem evidência de uso;
- acoplamento excessivo ao Jötunn;
- duplicação de patches/hot paths;
- configurações futuras com efeitos colaterais cruzados.

## Referências

- Issue #1
- AlcidesJr/Fix_Development
- docs/ARCHITECTURE.md
- docs/CONFLICTS.md
- docs/REFERENCE-PROJECTS.md
