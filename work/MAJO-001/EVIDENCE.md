# EVIDENCE — MAJO-001

## Intake

- Base HEAD: `3a5533085258dd86d700df84caa0e411cff20921`
- Task HEAD inicial: `3a5533085258dd86d700df84caa0e411cff20921`
- Issue: #4
- Branch: `task/MAJO-001-core-runtime`
- Dependências: PASS — MAJO-000 = DONE
- BLOCKED_BY: none
- DEFERRED_GATE: none

## Revalidação de plataforma — 2026-09-18

- Valheim oficial: patch `1.0.15` publicado em 2026-09-18.
- BepInEx upstream: `5.4.23.5`.
- BepInExPack_Valheim: `5.4.2350`, baseado em BepInEx `5.4.23.5`.
- Jötunn upstream: `2.30.1`.
- Changelog Jötunn: 2.30.0 atualizou a maioria dos sistemas para Valheim 1.0.7; 2.30.1 contém correções adicionais para o sistema de build do Valheim 1.0.
- `JotunnLib.csproj` v2.30.1: target `net462`.
- `GameVersions` v2.30.1 fornece versão semântica de Valheim.
- Jötunn documenta `GUIManager.IsHeadless()` para detecção precoce de dedicated/headless e `ZNetExtension` para local/client/server após ZNet.

Status: baseline candidata. Promoção depende dos gates da MAJO-001.

## Planejamento

- PLAN: `work/MAJO-001/PLAN.md`
- Arquitetura base: `docs/ARCHITECTURE.md`
- Contratos: `docs/{DEPENDENCIES,COMPATIBILITY,INPUT,PATCH-OWNERSHIP,SECURITY-BASELINE,VERSIONING,WORKFLOW}.md`
- Issue: #4

## Implementação

- Commits: pending
- Resumo: pending

## Verificação

| Gate | Comando/Run | Resultado |
| --- | --- | --- |
| Core tests | pending | pending |
| Runtime build | pending | pending |
| Artifact validation | pending | pending |
| Governance | pending | pending |
| CI | pending | pending |

## Review

- Resultado: pending
- Findings abertos: pending
- REVIEW.md: `work/MAJO-001/REVIEW.md`

## Segurança

- Resultado: pending
- Foco: trust boundary futura preservada, supply chain, logs, ausência de privileged RPC/filesystem/runtime updater.

## Integração

- PR: pending
- HEAD aprovado: pending
- Merge SHA: pending

## Fechamento

- STATUS.md: em andamento
- BOARD.md: em andamento
- Pendências formalizadas: none
