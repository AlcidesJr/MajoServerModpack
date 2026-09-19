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
- Jötunn 2.30.x contém a adaptação à linha Valheim 1.0.
- `JotunnLib` 2.30.1 usa `net462`.
- Jötunn fornece `GUIManager.IsHeadless()` e `ZNetExtension` para sinais de runtime.

Status: combinação validada pelo build automatizado desta tarefa; promoção canônica será registrada no closeout.

## Planejamento

- PLAN: `work/MAJO-001/PLAN.md`
- Arquitetura base: `docs/ARCHITECTURE.md`
- Contratos: `docs/{DEPENDENCIES,COMPATIBILITY,INPUT,PATCH-OWNERSHIP,SECURITY-BASELINE,VERSIONING,WORKFLOW}.md`
- Issue: #4

## Implementação

- CONTENT_HEAD: `8780223d7131569acf55a42d76802f3df50b3361`
- Runtime: BepInEx bootstrap + Jötunn hard dependency controlada.
- Core: ModuleRegistry, lifecycle, InputRegistry, PatchCoordinator, metadata e diagnostics.
- Platform: logging/framework metadata e detecção de contexto Valheim.
- Gameplay/RPC/config funcional: nenhum.

## Verificação

| Gate | Comando/Run | Resultado |
| --- | --- | --- |
| Governance | GitHub Actions run 35408340023 / job foundation | PASS |
| Core tests | GitHub Actions run 35408340023 / job core-tests | PASS |
| Runtime build | GitHub Actions run 35408340023 / job runtime-build | PASS |
| Artifact validation | run 35408340023 — somente `MajoServerModpack.dll` no artifact staging | PASS |
| CI | run 35408340023 @ `8780223d7131569acf55a42d76802f3df50b3361` | PASS |

O build provisionou Valheim Dedicated Server, BepInEx 5.4.23.5 com SHA-256 fixado, restaurou Jötunn 2.30.1 e compilou `net462`.

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

- STATUS.md: IN_REVIEW
- BOARD.md: IN_REVIEW
- Pendências formalizadas: none
