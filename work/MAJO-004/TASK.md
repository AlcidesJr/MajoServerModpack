# TASK — MAJO-004

## Título

Launcher

## Objetivo

Criar uma aplicação desktop separada para configurar o MajoServerModpack, validar o ambiente modded e iniciar o Valheim modded.

## Estado

PLANNED

## Dependências

- DEPENDS_ON: MAJO-003
- BLOCKED_BY: MAJO-003 not DONE
- DEFERRED_GATE: none
- Issue: #11

## Escopo futuro

- installation discovery;
- environment preflight;
- config editor baseado no schema MAJO-003;
- profiles;
- launch orchestration;
- diagnostics/log access.

## Fora de escopo arquitetural

- tornar launcher requisito do runtime;
- conceder autoridade/admin;
- substituir SecureRpcGateway;
- editar servidor remoto como fonte de verdade;
- updater irrestrito.

## Critérios de aceite da arquitetura

- [x] launcher separado do runtime documentado;
- [x] relação com MAJO-000/001/002/003/005 documentada;
- [x] trust boundary documentada;
- [x] dependência do schema MAJO-003 documentada;
- [ ] tecnologia desktop definida;
- [ ] implementação concluída;
- [ ] tests/review/security/merge/closeout concluídos.
