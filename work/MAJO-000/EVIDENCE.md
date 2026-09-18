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

- CONTENT_HEAD: pending — será fixado no commit substantivo que corrige o rereview atual
- Resumo: governança, arquitetura, catálogo funcional, segurança, ownership de patches e validação reproduzível.
- Runtime/gameplay: nenhum.

## Verificação

| Gate | Comando/Run | Resultado |
| --- | --- | --- |
| Fundação | `python3 tools/validate_foundation.py` — Actions run 35381039241 / job 105716990730 | PASS |
| PR foundation | `python3 tools/validate_foundation.py` + `git diff --check origin/main...HEAD` — Actions run 35381040248 / job 105716992448 | PASS |
| Catálogo | validador: 15 referências obrigatórias + ausência de `TBD` | PASS |
| Conflitos | validador: 7 categorias obrigatórias | PASS |
| Patch ownership | Actions runs 35381334678 (push) e 35381340295 (PR), HEAD `81edb782...` | PASS — validador com owner único executado |
| Push diff semantics | workflow atualizado para usar `github.event.before` em push e base SHA em PR | VERIFYING — requer run do novo CONTENT_HEAD |

## Review

- Review independente Codex em `81182f3246...`: FINDINGS — 3 × P2.
- Review independente Codex em `6826bb975c...`: FINDINGS — 1 × P2 adicional.
- Findings efetivos anteriores: 4 × P2, todos corrigidos.
- Rereview Codex em `81edb7823e...`: FINDINGS — 3 × P2 adicionais.
- P2 adicionais: semântica do diff em push para `main`; evidência prematura do ownership; transição obrigatória por `SECURITY_REVIEW`.
- Correções aplicadas ao conteúdo; verificação do workflow atualizada ainda pendente do próximo run.
- Novo rereview independente após CI: pending.
- REVIEW.md: `work/MAJO-000/REVIEW.md`.

## Segurança

- Resultado: PASS.
- Security content HEAD anterior: `b846eb0d4e09970a4550ff58c0f5cdb37204f052`; nova revalidação será registrada no estado `SECURITY_REVIEW` após review independente limpo.
- Findings de review interno: supply-chain pinning e default seguro para HTTP/WebSocket/WebMap.
- Mitigações: `docs/SECURITY-BASELINE.md`, ADR-011, ADR-012 e política de promoção em `docs/DEPENDENCIES.md`.
- P0/P1 abertos: 0.
- Segredos/runtime: nenhum adicionado.

## Integração

- PR: #2
- CONTENT_HEAD aprovado internamente: pending após correções do rereview
- HEAD exato de rereview: registrado pela timeline do PR #2 para evitar autorreferência Git.
- Merge SHA: pending

## Fechamento

- STATUS.md: em andamento
- BOARD.md: em andamento
- Pendências formalizadas: none
