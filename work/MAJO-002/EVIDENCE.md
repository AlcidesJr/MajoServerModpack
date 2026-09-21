# EVIDENCE — MAJO-002

## Intake

- main HEAD: `b9eb209a633721f3acdb028aed7377f2f7c34d58`;
- main workflow: governance run 35471672684 — PASS;
- MAJO-001: DONE;
- issue MAJO-001 #4: closed/completed;
- implementation merge MAJO-001: `31275f2a24679030fb2110f0eeea756eee51d159`;
- remediation merge: `cc7b273264c393473918b61b14f95aacde3725ff`;
- closeout merge: `36ab695506dd746e21cf1976f5ef56590782bbd2`;
- baseline-doc merge: `9160dee10c4b9b081bdbda303ef792987c275e4c`;
- PRs abertos no intake: 0;
- issue MAJO-002: #10;
- branch: `task/MAJO-002-authority-secure-networking`;
- BLOCKED_BY: none;
- DEFERRED_GATE: none.

## APIs/plataforma

Jötunn 2.30.1 revalidado:

- `ModCompatibility.ZNet_OnNewConnection` registra RPC diretamente em `peer.m_rpc`;
- callback direto recebe o `ZRpc` real;
- `ZNetExtension.IsAdmin` usa peer/socket e `m_adminList` no servidor;
- `CustomRPC` recebe sender routed id, logo não é a trust boundary primária;
- `NetworkCompatibility(EveryoneMustHaveMod, None)` permite exigir presença sem usar `MajoVersion` como wire contract.

## Planejamento

- `TASK.md` e `PLAN.md` criados;
- trust boundary: conexão real → TrustedPeerContext → SecureRpcGateway;
- threat model registrado em `PLAN.md`.

## Implementação

- CONTENT_HEAD: `d2b1e700d5d6f7654b3f0be381ad545f723a423f`;
- MajoVersion: `0.0.2`;
- ProtocolVersion: `1`;
- capability obrigatória: `Core.Network.v1`;
- transport: RPC direto por `ZNetPeer.m_rpc`, bound ao `ZRpc` real;
- Core: `SecureRpcGateway`, `OperationRegistry`, sessions, authorization, payload validation, replay protection, rate limiting, audit e failure isolation;
- Platform: bind/unbind peer, handshake, gate de `RPC_PeerInfo`, admin resolution server-side e erro público de incompatibilidade;
- gameplay/config UI/MAJO-003+: não implementados.

### Hardening durante implementação

1. runtime-build inicial falhou por `Harmony.UnpatchAll(string)` obsoleto; corrigido para `UnpatchSelf()`;
2. quota global foi elevada para todo envelope válido decodificado, não apenas `Request`;
3. `OperationDescriptor` passou a rejeitar direction/execution-side incoerentes;
4. regressões adicionadas para flood de mensagens não-`Request` e metadata de direction inválida;
5. pre-review posterior identificou que `Response` e `Error` ainda podiam ser processados com sessão Pending. Correção em andamento: somente Hello/HelloAck e erro de rejeição server→client serão aceitos antes de compatibilidade.

## Verificação

| Gate | Evidência | Resultado |
| --- | --- | --- |
| Foundation/governance | run 35633905927 | PASS |
| Core tests | job 106446545294 | PASS |
| Runtime build | job 106446673340 | PASS |
| Artifact validation | job 106446673340 | PASS |
| CONTENT_HEAD | `d2b1e700d5d6f7654b3f0be381ad545f723a423f` | PASS |

Primeiro run completo relevante `35633364610`: core-tests PASS e runtime-build FAIL por API Harmony obsoleta causada pelo diff. Foi tratado como regressão da MAJO-002, corrigido e não classificado como DEFERRED_GATE.

Run decisivo do CONTENT_HEAD: `35633905927` — PASS.

## Review

pending

## Security review

pending

## Integração

pending
