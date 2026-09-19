# EVIDENCE — MAJO-001

## Intake

- Base HEAD: `3a5533085258dd86d700df84caa0e411cff20921`
- Issue: #4
- Branch: `task/MAJO-001-core-runtime`
- Dependências: PASS — MAJO-000 = DONE
- BLOCKED_BY: none
- DEFERRED_GATE: none

## Baseline promovida pela tarefa

- Valheim: `1.0.15`
- Valheim Dedicated Server build ID: `25390671`
- BepInEx: `5.4.23.5`
- BepInExPack_Valheim: `5.4.2350`
- Jötunn: `2.30.1`
- Target: `net462`

O CI baixa a build corrente via SteamCMD e falha se o appmanifest não corresponder ao build ID aprovado.

## Implementação

- CONTENT_HEAD: `0d4c6f0c4cfd87d3c900d85f781f5270308131c0`
- Runtime: BepInEx bootstrap + Jötunn hard dependency controlada.
- Core: ModuleRegistry/lifecycle, InputRegistry, PatchCoordinator, metadata e diagnostics.
- Platform: logging/framework metadata e detecção de contexto Valheim.
- CI: build real, BepInEx SHA-256 fixado, build ID Valheim validado e retry limitado de SteamCMD.
- Gameplay/RPC/config funcional: nenhum.

## Verificação

| Gate | Evidência | Resultado |
| --- | --- | --- |
| Governance | push run 35448757393 / PR run 35448759291 | PASS |
| Core tests | runs 35448757393 e 35448759291 | PASS |
| Runtime build | runs 35448757393 e 35448759291 | PASS |
| Artifact validation | somente `MajoServerModpack.dll` em staging | PASS |
| Valheim build pin | build ID `25390671` validado no appmanifest | PASS |
| Review final | Codex, reviewed commit `0d4c6f0c4c` | PASS |
| Threads abertas | PR #5 | 0 |

## Findings tratados

1. Bootstrap após Shutdown — corrigido em `301cba14f5ef5d9d3234d6b4856baf12b5c88aea`.
2. Patch surface whitespace — corrigido em `2fcd2263d6d5572b69038699ce05f3718ba697f1`.
3. Reinicialização de módulos iniciados — corrigido em `5c5cb462c682026f7c65affe07555ec4dcc86349`, regressão em `189ba3a71a6cad27a7bda303a7b2e31c288e2e4e`.
4. Build Valheim não fixada/verificada — corrigido em `0d4c6f0c4cfd87d3c900d85f781f5270308131c0`.
5. Instabilidade transitória SteamCMD — retry limitado em `c8065ec4c79b31d28905994919dd34dd20f3ed4a`.

## Segurança

SECURITY_REVIEW: PASS.

Revisão do diff e superfícies:
- sem RPC/autoridade implementados;
- execution context é diagnóstico, não autorização;
- sem filesystem arbitrário;
- sem updater/download em runtime;
- sem Harmony patch funcional;
- sem secrets/tokens/passwords no código;
- dependências externas isoladas e versionadas;
- BepInEx download de CI validado por SHA-256;
- Valheim build validada por build ID.

## Integração

- PR: #5
- HEAD aprovado: `0d4c6f0c4cfd87d3c900d85f781f5270308131c0`
- Merge SHA: pending

## Fechamento

- STATUS.md: SECURITY_REVIEW
- BOARD.md: SECURITY_REVIEW
- Pendências formalizadas: none
