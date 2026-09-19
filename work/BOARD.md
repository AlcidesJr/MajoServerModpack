# Board

| TASK | Título | Estado | Dependências | Issue | PR | BLOCKED_BY | DEFERRED_GATE |
| --- | --- | --- | --- | --- | --- | --- | --- |
| MAJO-000 | Foundation, governance and architecture | DONE | none | #1 | #2 / #3 | none | none |
| MAJO-001 | Core runtime | SECURITY_REVIEW | MAJO-000 | #4 | #5 | none | none |
| MAJO-002 | Authority and secure networking | PLANNED | MAJO-001 | pending | pending | none | none |
| MAJO-003 | Configuration platform | PLANNED | MAJO-001, MAJO-002 | pending | pending | none | none |
| MAJO-004 | Majo Control Panel | PLANNED | MAJO-003 | pending | pending | none | none |

## Regras

- Uma linha por tarefa relevante.
- Não antecipar implementação de tarefa futura.
- Detalhes vivem em `work/<TASK>/`.
- Novos módulos funcionais serão formalizados somente após o catálogo/conflitos da MAJO-000.
