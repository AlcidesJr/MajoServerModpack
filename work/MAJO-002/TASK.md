# TASK — MAJO-002

## Título

Authority and secure networking

## Objetivo

Construir a trust boundary central do Majo para identidade, compatibilidade, autorização e dispatch seguro de mensagens, sem implementar gameplay.

## Dependências

- DEPENDS_ON: MAJO-001 — DONE
- BASE_HEAD: `b9eb209a633721f3acdb028aed7377f2f7c34d58`
- Issue: #10
- BLOCKED_BY: none
- DEFERRED_GATE: none

## Regra de confiança

`CLIENTE NÃO É FONTE DE VERDADE`.

Claims de actor, admin, Steam/network id, target ou permission vindos do payload não concedem autoridade. A identidade usada pelo gateway deve nascer do peer/RPC real da conexão.

## Escopo

- handshake Majo cliente ↔ servidor;
- `ProtocolVersion` independente de `MajoVersion`;
- sessão por conexão;
- identidade confiável de transporte;
- `SecureRpcGateway` e `OperationRegistry`;
- direction/allowed execution side;
- permissões e autorização server-side;
- envelope binário versionado;
- validação estrutural/semântica e limites;
- rate limiting por conexão/operação;
- replay/duplicação limitada por request id;
- error model seguro;
- audit básico sem payload integral;
- failure isolation;
- cleanup em disconnect/reconnect;
- compatibilidade conservadora;
- integração controlada com Valheim/Jötunn;
- testes puros e regressões de segurança.

## Fora de escopo

MAJO-003+, configuração funcional/UI, painel administrativo, kick/ban/teleport/spawn, gameplay, WebMap, inventory/storage/building, audit database, HTTP/WebSocket, compressão avançada e otimizações ZDO.

## Critérios de aceite

- [ ] handshake Majo implementado;
- [ ] protocolo versionado e separado da versão pública;
- [ ] identidade vinculada ao `ZNetPeer/ZRpc` da conexão;
- [ ] `SecureRpcGateway` e `OperationRegistry` implementados;
- [ ] autorização server-side e permissions infrastructure;
- [ ] payload validation e limites;
- [ ] rate limiting e replay/duplicate;
- [ ] handler failure isolation;
- [ ] session cleanup;
- [ ] compatibility rejection segura;
- [ ] audit básico;
- [ ] testes unitários/de segurança e CI PASS;
- [ ] review independente PASS;
- [ ] SECURITY_REVIEW PASS;
- [ ] implementação e closeout integrados;
- [ ] Board atualizado e issue #10 encerrada.

## Referências

`AGENTS.md`, `docs/ARCHITECTURE.md`, `docs/SECURITY-BASELINE.md`, `docs/PATCH-OWNERSHIP.md`, `docs/COMPATIBILITY.md`, `docs/DEPENDENCIES.md`, `docs/WORKFLOW.md`, `work/MAJO-000/`, `work/MAJO-001/`.
