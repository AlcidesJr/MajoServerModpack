# REVIEW — MAJO-001

## Escopo revisado

- TASK: `work/MAJO-001/TASK.md`
- PLAN: `work/MAJO-001/PLAN.md`
- CONTENT_HEAD: `0d4c6f0c4cfd87d3c900d85f781f5270308131c0`
- Diff: `main...task/MAJO-001-core-runtime`

## Resultado

REVIEW: PASS
SECURITY_REVIEW: PASS

## Evidência

- push run 35448757393: PASS;
- PR run 35448759291: PASS;
- Codex final: reviewed commit `0d4c6f0c4c`, sem problemas relevantes;
- review threads abertas: 0.

## Findings tratados

- P2 — rejeitar Bootstrap após Shutdown;
- P2 — normalizar patch surfaces antes de ownership;
- P2 — fixar/verificar build do Valheim no CI;
- P2 — impedir reinicialização de módulos fora de Registered.

Todos tratados com código/testes/evidência e threads resolvidas.

## Verificações

- [x] Critérios de aceite cobertos
- [x] Compatibilidade/contratos revisados
- [x] Edge cases relevantes avaliados
- [x] Testes adequados ao risco
- [x] Sem expansão indevida de escopo
- [x] Documentação consistente
- [x] Evidências correspondem ao CONTENT_HEAD
- [x] Revisão independente concluída
- [x] Findings efetivos tratados
- [x] SECURITY_REVIEW concluído após rereview

## Segurança

PASS.

- nenhuma ação privilegiada implementada;
- execution context não concede autoridade;
- nenhum RPC/admin/filesystem/updater funcional;
- nenhum segredo registrado;
- dependências de framework permanecem externas;
- supply chain do CI fixa BepInEx por SHA-256 e Valheim por build ID;
- nenhuma feature MAJO-002 foi antecipada.

## Conclusão

A MAJO-001 está READY_TO_MERGE após a remediação descrita abaixo.

## Reabertura por review tardio — 2026-09-19

- P2 — `ValheimExecutionContextProvider` buscava `ZNet.m_openServer` como campo estático e lia com alvo nulo.
- Impacto: em mundo local aberto para outros jogadores, o runtime podia reportar `LocalWorld` em vez de `ListenServer`.
- Origem: thread publicada no PR #5 após o merge dos PRs #5 e #6.
- Correção: commit `72855d45d413d5bcc9e9b0a9eed7c2acbc43df3e` busca o campo com `BindingFlags.Instance` e lê seu valor na instância `ZNet` atual.
- Verificação local: governance PASS, testes puros PASS e `git diff --check` PASS.
- CI: push run 35450718574 PASS; PR run 35450722406 PASS.
- Rereview Codex do HEAD `944c7001068fd1f18730f98ff1b0c3e897e75ced`: PASS, sem findings.
- Threads abertas nos PRs #5, #6 e #7: 0.
- SECURITY_REVIEW: PASS — a mudança não amplia autoridade, entrada não confiável, filesystem, rede, processos, secrets ou supply chain.
- Estado: READY_TO_MERGE.

## Integração da remediação

- PR #7: MERGED.
- HEAD final: `75e2ec99fd318ea4ac7636d3aeb5b5735e218a31`.
- Merge SHA: `cc7b273264c393473918b61b14f95aacde3725ff`.
- Main pós-merge run 35451415347: PASS.
- Findings abertos: 0.
- SECURITY_REVIEW: PASS.
