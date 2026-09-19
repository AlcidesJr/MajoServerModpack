# EVIDENCE — MAJO-001

## Intake

- Base HEAD: `3a5533085258dd86d700df84caa0e411cff20921`
- Issue: #4
- Branch: `task/MAJO-001-core-runtime`
- Dependências: PASS — MAJO-000 = DONE
- BLOCKED_BY: none
- DEFERRED_GATE: none

## Baseline promovida

- Valheim: `1.0.15`
- Valheim Dedicated Server build ID: `25390671`
- BepInEx: `5.4.23.5`
- BepInExPack_Valheim: `5.4.2350`
- Jötunn: `2.30.1`
- Target: `net462`

## Implementação

- CONTENT_HEAD: `0d4c6f0c4cfd87d3c900d85f781f5270308131c0`
- Runtime: bootstrap BepInEx + Jötunn hard dependency controlada.
- Core: ModuleRegistry/lifecycle, InputRegistry, PatchCoordinator, metadata e diagnostics.
- Platform: logging/framework metadata e detecção de contexto Valheim.
- CI: build real, BepInEx SHA-256 fixado, Valheim build ID validado, retry limitado de SteamCMD.
- Gameplay/RPC/config funcional: nenhum.

## Verificação

| Gate | Evidência | Resultado |
| --- | --- | --- |
| Governance | push run 35448757393 / PR run 35448759291 | PASS |
| Core tests | runs 35448757393 e 35448759291 | PASS |
| Runtime build | runs 35448757393 e 35448759291 | PASS |
| Artifact validation | somente `MajoServerModpack.dll` em staging | PASS |
| Metadata HEAD final | push run 35449209362 / PR run 35449212164 | PASS |
| Review final | Codex reviewed commit `0d4c6f0c4c` | PASS |
| Security review | diff/superfícies da MAJO-001 | PASS |
| Threads abertas | PR #5 | 0 |

## Findings tratados

1. Bootstrap após Shutdown — `301cba14f5ef5d9d3234d6b4856baf12b5c88aea`.
2. Patch surface whitespace — `2fcd2263d6d5572b69038699ce05f3718ba697f1`.
3. Reinicialização de módulos — `5c5cb462c682026f7c65affe07555ec4dcc86349` + regressão `189ba3a71a6cad27a7bda303a7b2e31c288e2e4e`.
4. Valheim build pin/verification — `0d4c6f0c4cfd87d3c900d85f781f5270308131c0`.
5. Retry transitório SteamCMD — `c8065ec4c79b31d28905994919dd34dd20f3ed4a`.

## Segurança

SECURITY_REVIEW: PASS.

- sem RPC/autoridade funcional;
- execution context não concede autorização;
- sem filesystem arbitrário;
- sem updater/download em runtime;
- sem Harmony patch funcional;
- sem secrets/tokens/passwords;
- BepInEx validado por SHA-256 no CI;
- Valheim validado por build ID;
- nenhuma feature MAJO-002 antecipada.

## Integração

- Implementation PR: #5
- Implementation merge SHA: `31275f2a24679030fb2110f0eeea756eee51d159`
- Issue #4: closed/completed
- Closeout PR: #6

## Fechamento

- STATUS.md: DONE
- BOARD.md: DONE
- TASK acceptance: completo
- Pendências: none

## Reabertura por review tardio — 2026-09-19

- O review automático publicou, após os merges dos PRs #5 e #6, um P2 aberto em `ValheimExecutionContextProvider`.
- Problema: `ZNet.m_openServer` é campo de instância, mas era buscado com `BindingFlags.Static` e lido com alvo nulo.
- Issue #4: reaberta para remediação explícita.
- Branch: `task/MAJO-001-late-review-remediation`.
- Remediation PR: #7.
- Escopo: corrigir somente o finding, repetir CI/review/security e refazer o closeout.
- BLOCKED_BY: none.
- DEFERRED_GATE: none.

### Correção

- CONTENT_HEAD: `72855d45d413d5bcc9e9b0a9eed7c2acbc43df3e`.
- `m_openServer` é buscado com `BindingFlags.Instance` e lido a partir da instância `ZNet` atual.
- `python tools/validate_foundation.py`: PASS.
- `dotnet run --project tests/MajoServerModpack.Core.Tests/MajoServerModpack.Core.Tests.csproj -c Release`: PASS, 16 cenários.
- `git diff --check`: PASS.
- Push run 35450718574: governance, core tests, runtime build e artifact validation PASS.
- PR run 35450722406: governance, core tests, runtime build e artifact validation PASS.
- Rereview Codex do HEAD `944c7001068fd1f18730f98ff1b0c3e897e75ced`: PASS, sem findings.
- SECURITY_REVIEW: PASS.
- Threads abertas nos PRs #5, #6 e #7: 0.

### Segurança da remediação

- Mudança restrita à leitura reflectiva de um campo booleano já existente na instância `ZNet`.
- O execution context continua sendo diagnóstico e não concede autoridade.
- Nenhum RPC, permissão, filesystem, processo, download em runtime, secret ou patch funcional foi adicionado.
- Supply chain, versões e validações de artefato permanecem inalteradas.

## Integração da remediação

- Remediation PR: #7.
- Remediation HEAD final: `75e2ec99fd318ea4ac7636d3aeb5b5735e218a31`.
- Remediation merge SHA: `cc7b273264c393473918b61b14f95aacde3725ff`.
- Main pós-merge run 35451415347: governance, core tests, runtime build e artifact validation PASS.
- Resultado: MERGED.

## Closeout final proposto

- Branch: `task/MAJO-001-final-closeout`.
- Closeout PR: #8.
- STATUS final proposto: DONE.
- BOARD final proposto: DONE.
- Issue #4: será encerrada automaticamente quando o closeout for integrado.
- BLOCKED_BY: none.
- DEFERRED_GATE: none.

Após o merge do closeout, `main` passa a conter o fechamento canônico da MAJO-001 em DONE.
