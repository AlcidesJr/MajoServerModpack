# REVIEW — MAJO-002

## Estado

REVIEW: pending
SECURITY: pending

## Escopo obrigatório

- identidade vinculada à conexão real;
- nenhuma autorização por claim do cliente;
- handshake/protocol/capabilities;
- OperationRegistry/direction;
- authn/authz e admin server-side;
- codec/size/truncation;
- validação semântica;
- rate limiting e bypass;
- replay/duplicate;
- session cleanup;
- handler failure isolation;
- error leakage;
- audit/log flooding;
- thread boundary;
- patch ownership;
- dedicated/listen/local;
- ausência de gameplay/MAJO-003+.

## Política

P0/P1 aberto bloqueia merge. P2 que comprometa trust boundary também deve ser corrigido antes do merge. Findings devem registrar severity, impact, fix, test, evidence e status.

## Reviewer independente

O PR será submetido ao `chatgpt-codex-connector`, já configurado no repositório. Não registrar PASS antes do review do HEAD aplicável.
