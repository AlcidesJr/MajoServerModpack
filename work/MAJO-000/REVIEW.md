# REVIEW — MAJO-000

## Escopo revisado

- TASK: `work/MAJO-000/TASK.md`
- PLAN: `work/MAJO-000/PLAN.md`
- CONTENT_HEAD: `3aa0b5bc49ad2801c56710f5af07bfd5823c9054`
- Diff: `main...task/MAJO-000-foundation-governance`

## Resultado

REVIEW: FINDINGS_RESOLVED — rereview independente final pending

### Autorrevisão

PASS — escopo documental coerente; nenhum runtime/gameplay entrou no diff.

## Findings

### P2 — HEAD auditável

- Origem: Codex review no commit `81182f3246...`.
- Impacto: estado não identificava corretamente o conteúdo submetido.
- Correção: `CONTENT_HEAD` explícito + política de metadata-only e review tip na timeline do PR.
- Status: RESOLVED.

### P2 — Verificação reproduzível

- Origem: Codex review no commit `81182f3246...`.
- Impacto: PASS anterior não apontava comando/run reproduzível.
- Correção: `tools/validate_foundation.py` + workflow `governance`; runs 35381039241 e 35381040248 PASS.
- Status: RESOLVED.

### P2 — Catálogo com referências TBD

- Origem: Codex review no commit `81182f3246...`.
- Impacto: critério de catálogo profundo estava marcado sem concluir Armoire/GearAndStorage.
- Correção: Armoire classificado por appearance/persistência/peer compatibility; GearAndStorage desmembrado por inventory/storage/production/build/portal/quests.
- Status: RESOLVED.

### P2 — Owner ambíguo em patches críticos

- Origem: Codex review no commit `6826bb975c...`.
- Impacto: risco de patches concorrentes em ZRoutedRpc e containers.
- Correção: owner único `Core.SecureRpcGateway`; containers separados entre `Core.WorldIndex` e `Storage.ContainerTransactions`; validador rejeita owners compostos.
- Status: RESOLVED.

### P2 — Diff vazio em push para main

- Origem: Codex rereview no commit `81edb7823e...`.
- Impacto: `origin/main...HEAD` em push de `main` compara o commit consigo mesmo e pode deixar whitespace errors passarem.
- Correção: workflow distingue `pull_request` de `push`; push usa `github.event.before → HEAD` com fallback seguro para branch nova/root.
- Status: RESOLVED — runs 35382076587 (push) e 35382081125 (PR) PASS.

### P2 — Evidência prematura do ownership

- Origem: Codex rereview no commit `81edb7823e...`.
- Impacto: EVIDENCE declarava PASS sem associar o novo check a um run.
- Correção: runs 35381334678 e 35381340295, ambos no HEAD `81edb782...`, executaram o validador com a regra de owner único e passaram.
- Status: RESOLVED.

### P2 — Transição obrigatória por SECURITY_REVIEW

- Origem: Codex rereview no commit `81edb7823e...`.
- Impacto: próximo gate textual pulava o estado obrigatório `SECURITY_REVIEW`.
- Correção: fluxo agora exige rereview limpo → `SECURITY_REVIEW` explícito → revalidação → `READY_TO_MERGE`.
- Status: RESOLVED.

### P2 — Primeiro push de branch nova verificava apenas o último commit

- Origem: Codex rereview no commit `82a8cf8a58...`.
- Impacto: em branch recém-criada, `github.event.before=000...` levava ao fallback `HEAD^ HEAD`, cobrindo apenas o último commit.
- Correção: primeiro push agora compara `HEAD` com `merge-base(origin/<default-branch>, HEAD)`; árvore vazia é usada apenas sem baseline.
- Evidência: runs 35388497710 (push) e 35388501155 (PR), ambos PASS.
- Status: RESOLVED.

## Verificações

- [x] Critérios de aceite funcionais/documentais cobertos
- [x] Compatibilidade/contratos revisados
- [x] Edge cases arquiteturais relevantes avaliados
- [x] Sem expansão indevida de escopo runtime
- [x] Documentação consistente no CONTENT_HEAD
- [x] Evidências reproduzíveis associadas a runs GitHub
- [x] CI do workflow corrigido PASS
- [ ] Rereview independente sem findings efetivos pendentes

## Segurança

SECURITY: PASS

- trust boundaries cliente/servidor documentadas;
- privileged RPC fail-closed;
- supply chain de frameworks fixada/promovida manualmente;
- HTTP/WebSocket/WebMap seguro por padrão;
- persistência/import/export com regras de confinamento e validação;
- nenhum segredo ou código runtime neste PR.

## Conclusão

Todos os findings conhecidos estão corrigidos e verificados. Falta rereview independente final; depois a tarefa deve passar explicitamente por SECURITY_REVIEW antes de READY_TO_MERGE.
