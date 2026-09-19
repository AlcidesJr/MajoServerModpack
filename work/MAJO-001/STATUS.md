# STATUS — MAJO-001

TASK: MAJO-001
STATE: DONE
BRANCH: task/MAJO-001-baseline-consistency
BASE_HEAD: 3a5533085258dd86d700df84caa0e411cff20921
CONTENT_HEAD: 3a10130d0f2e19bc203f1026830c34a4a5e89047
PR: #5 / #7 / #8 / #9
ISSUE: #4
IMPLEMENTATION_MERGE_SHA: 31275f2a24679030fb2110f0eeea756eee51d159
REMEDIATION_MERGE_SHA: cc7b273264c393473918b61b14f95aacde3725ff
CLOSEOUT_PR: #8
CLOSEOUT_MERGE_SHA: 36ab695506dd746e21cf1976f5ef56590782bbd2
BASELINE_DOC_MERGE_SHA: 9160dee10c4b9b081bdbda303ef792987c275e4c
BLOCKED_BY: none
DEFERRED_GATE: none
SECURITY: PASS

## Resultado

MAJO-001 concluída.

- finding tardio corrigido no CONTENT_HEAD: leitura de `ZNet.m_openServer` usa o campo de instância e o objeto `ZNet` atual;
- implementação anterior integrada via PR #5, merge SHA `31275f2a24679030fb2110f0eeea756eee51d159`;
- closeout anterior integrado via PR #6, merge SHA `52a479b45207b8ead0c764bb5cf977439b4134d2`;
- remediação integrada via PR #7, merge SHA `cc7b273264c393473918b61b14f95aacde3725ff`;
- CI pós-merge no `main`: run 35451415347 PASS;
- validação local: governance PASS, testes puros PASS, `git diff --check` PASS;
- CI do PR #7: push run 35450718574 PASS; PR run 35450722406 PASS;
- Codex rereview do HEAD `944c7001068fd1f18730f98ff1b0c3e897e75ced`: PASS, sem findings;
- SECURITY_REVIEW da remediação: PASS;
- threads abertas nos PRs #5, #6 e #7: 0;
- issue #4: closed/completed;
- closeout final integrado via PR #8, merge SHA `36ab695506dd746e21cf1976f5ef56590782bbd2`;
- CI do closeout no HEAD `a5346a3e3052a9afb673250c32781a3addd18f64`: push run 35451638958 PASS; PR run 35451641640 PASS;
- finding: a baseline havia sido descrita como promovida/suportada sem evidência de smoke dentro do Valheim;
- correção: registrar build PASS, smoke runtime NOT_RUN e manter a combinação como candidata;
- o smoke não era critério de aceite da implementação da MAJO-001 e não é apresentado como gate satisfeito ou adiado;
- correção documental integrada via PR #9, merge SHA `9160dee10c4b9b081bdbda303ef792987c275e4c`;
- Codex review do CONTENT_HEAD documental `3a10130d0f`: PASS, sem novos findings;
- metadata HEAD `543fb078df266c76b4a09fc2d844ee734b9ab45b`: CI PASS;
- SECURITY_REVIEW: N/A para a correção exclusivamente documental; o PASS da implementação permanece válido;
- pendências da MAJO-001: none.

## Baseline candidata validada para build

- Valheim `1.0.15`;
- Valheim Dedicated Server build ID `25390671`;
- BepInEx `5.4.23.5`;
- BepInExPack_Valheim `5.4.2350`;
- Jötunn `2.30.1`;
- target `net462`.

Runtime smoke: NOT_RUN. A combinação não está promovida como suportada em runtime.

## Limite de escopo

Nenhuma implementação da MAJO-002 ou tarefa futura faz parte desta remediação.
