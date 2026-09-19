# STATUS — MAJO-001

TASK: MAJO-001
STATE: READY_TO_MERGE
BRANCH: task/MAJO-001-late-review-remediation
BASE_HEAD: 3a5533085258dd86d700df84caa0e411cff20921
CONTENT_HEAD: 72855d45d413d5bcc9e9b0a9eed7c2acbc43df3e
PR: #7
ISSUE: #4
IMPLEMENTATION_MERGE_SHA: 31275f2a24679030fb2110f0eeea756eee51d159
BLOCKED_BY: none
DEFERRED_GATE: none
SECURITY: PASS

## Resultado

MAJO-001 reaberta explicitamente após um finding P2 tardio, publicado pelo review automático depois dos merges dos PRs #5 e #6.

- implementação integrada via PR #5;
- finding corrigido no CONTENT_HEAD: leitura de `ZNet.m_openServer` agora usa o campo de instância e o objeto `ZNet` atual;
- implementação anterior integrada via PR #5, merge SHA `31275f2a24679030fb2110f0eeea756eee51d159`;
- closeout anterior integrado via PR #6, merge SHA `52a479b45207b8ead0c764bb5cf977439b4134d2`;
- issue #4: reaberta;
- validação local: governance PASS, testes puros PASS, `git diff --check` PASS;
- CI do PR #7: push run 35450718574 PASS; PR run 35450722406 PASS;
- Codex rereview do HEAD `944c7001068fd1f18730f98ff1b0c3e897e75ced`: PASS, sem findings;
- SECURITY_REVIEW da remediação: PASS;
- threads abertas nos PRs #5, #6 e #7: 0;
- próximo gate: merge do PR #7 e novo closeout.

## Baseline promovida

- Valheim `1.0.15`;
- Valheim Dedicated Server build ID `25390671`;
- BepInEx `5.4.23.5`;
- BepInExPack_Valheim `5.4.2350`;
- Jötunn `2.30.1`;
- target `net462`.

## Limite de escopo

Nenhuma implementação da MAJO-002 ou tarefa futura faz parte desta remediação.
