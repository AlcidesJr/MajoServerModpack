# EVIDENCE — MAJO-000

## Intake

- Base HEAD: `d8756506887a54375732828143ea13b69e164b2a`
- Task HEAD inicial: `d8756506887a54375732828143ea13b69e164b2a`
- Dependências: PASS — none
- BLOCKED_BY: none
- DEFERRED_GATE: none
- Issue: #1

## Planejamento

- PLAN: `work/MAJO-000/PLAN.md`
- Decisões: `docs/DECISIONS.md`

## Implementação

- CONTENT_HEAD: `3aa0b5bc49ad2801c56710f5af07bfd5823c9054`
- Resumo: governança, arquitetura, catálogo funcional, segurança, ownership de patches e validação reproduzível.
- Runtime/gameplay: nenhum.

## Verificação

| Gate | Comando/Run | Resultado |
| --- | --- | --- |
| Fundação | `python3 tools/validate_foundation.py` — Actions run 35381039241 / job 105716990730 | PASS |
| PR foundation | `python3 tools/validate_foundation.py` + `git diff --check origin/main...HEAD` — Actions run 35381040248 / job 105716992448 | PASS |
| Catálogo | validador: 15 referências obrigatórias + ausência de `TBD` | PASS |
| Conflitos | validador: 7 categorias obrigatórias | PASS |
| Patch ownership | Actions runs 35381334678 e 35381340295 | PASS |
| Push diff semantics | Actions run 35382076587 | PASS |
| PR diff semantics | Actions run 35382081125 | PASS |
| First branch push semantics | Actions runs 35388497710 e 35388501155 | PASS |
| Metadata tip after final fix | Actions runs 35388591852 e 35388596451 | PASS |

## Review

- Codex `81182f3246...`: 3 × P2 — RESOLVED.
- Codex `6826bb975c...`: 1 × P2 — RESOLVED.
- Codex `81edb7823e...`: 3 × P2 — RESOLVED.
- Codex `82a8cf8a58...`: 1 × P2 — RESOLVED.
- Rereview final no tip `fba306a016ee53b7bded77e58b0cd21baa006b93`: PASS — “Didn't find any major issues”.
- Reação Codex no PR: +1 em 2026-09-18T19:58:27Z.
- Threads abertas após rereview: 0.
- REVIEW.md: `work/MAJO-000/REVIEW.md`.

## Segurança

- Estado atual: SECURITY_REVIEW.
- Baseline: `docs/SECURITY-BASELINE.md`.
- Controles já documentados: peer identity server-side, fail-closed RPC, supply-chain pinning, serviços HTTP/WebSocket/WebMap seguros por padrão, persistência/import/export confinados.
- Mudanças posteriores ao security hardening: workflow/governança e metadata; nenhuma nova superfície runtime.
- Revalidação formal no estado SECURITY_REVIEW: pending.

## Integração

- PR: #2
- CONTENT_HEAD: `3aa0b5bc49ad2801c56710f5af07bfd5823c9054`
- Review tip aprovado: `fba306a016ee53b7bded77e58b0cd21baa006b93`
- Merge SHA: pending

## Fechamento

- STATUS.md: em andamento
- BOARD.md: em andamento
- Pendências formalizadas: none
