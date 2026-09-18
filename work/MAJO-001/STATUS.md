# STATUS — MAJO-001

TASK: MAJO-001
STATE: IMPLEMENTING
BRANCH: task/MAJO-001-core-runtime
BASE_HEAD: 3a5533085258dd86d700df84caa0e411cff20921
HEAD: pending
PR: pending
ISSUE: #4
BLOCKED_BY: none
DEFERRED_GATE: none
SECURITY: pending

## Próximo gate

Concluir implementação e mover para VERIFYING após testes focados, build runtime e validação do CI.

## Última atualização

2026-09-18 — implementação do core runtime, adapters mínimos, harness de testes, documentação de build/runtime e pipeline de build adicionados à branch.

## Observações operacionais

- Nenhum Harmony patch funcional foi adicionado.
- Nenhuma feature MAJO-002+ foi implementada.
- Valheim/BepInEx/Jötunn permanecem dependências externas; DLLs proprietárias do jogo não são versionadas.
- Baseline permanece candidata até os gates de verificação.
