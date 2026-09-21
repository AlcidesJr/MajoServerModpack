# PLAN — MAJO-004

STATE: PLANNED

A implementação ainda não está autorizada.

## Sequência esperada

1. aguardar MAJO-003 DONE;
2. reconsultar o schema/config contract real;
3. decidir tecnologia desktop com dependências mínimas;
4. definir installation discovery e launch mechanism;
5. definir profile/config storage;
6. implementar preflight;
7. implementar editor dinâmico por schema;
8. implementar launch orchestration;
9. testar filesystem, quoting, schema compatibility e failure modes;
10. review independente e SECURITY_REVIEW.

## Decisões já fechadas

- aplicação separada;
- runtime funciona sem launcher;
- launcher não é trust boundary;
- não duplicar catálogo de settings;
- sem updater irrestrito no MVP.
