# PLAN — MAJO-002

## Solução

Adicionar lógica pura em `Majo.Core` e um adapter pequeno em `Majo.Platform.Network`.

O adapter registra um RPC direto no `ZRpc` de cada `ZNetPeer` durante `ZNet.OnNewConnection`. O callback fica vinculado à conexão real e cria um `TrustedPeerContext` antes de interpretar qualquer dado enviado pelo cliente.

Jötunn permanece framework de plataforma. `NetworkCompatibility(EveryoneMustHaveMod, None)` será proteção complementar para presença do mod; compatibilidade do wire contract continua sendo decidida pelo handshake Majo via `ProtocolVersion` e capability.

## Estado fresco

- main: `b9eb209a633721f3acdb028aed7377f2f7c34d58`;
- MAJO-001: DONE;
- issue MAJO-001 #4: closed/completed;
- main workflow run 35471672684: PASS;
- PRs abertos no intake: 0;
- issue MAJO-002: #10;
- branch: `task/MAJO-002-authority-secure-networking`;
- BLOCKED_BY: none;
- DEFERRED_GATE: none.

## APIs revalidadas

Jötunn 2.30.1 confirma:

- `ModCompatibility` registra RPC em `peer.m_rpc` no `ZNet.OnNewConnection`;
- o callback direto recebe `ZRpc`, portanto pode ser associado à conexão;
- `ZNetExtension.IsAdmin` resolve o peer no servidor e consulta a identidade do socket contra `ZNet.m_adminList`;
- `SynchronizationManager.PlayerIsAdmin` é estado local sincronizado e não será trust boundary;
- `CustomRPC` usa `ZRoutedRpc` e fornece um `long sender`; não será fonte primária de identidade privilegiada.

## Trust boundary

```text
client bytes (untrusted)
        |
        v
ZRpc bound to concrete ZNetPeer
        |
        v
Majo.Platform.Network
  connection id generated locally
  peer uid from ZNetPeer
  admin status resolved server-side
        |
        v
TrustedPeerContext
        |
        v
SecureRpcGateway
 parse -> session -> protocol -> operation -> replay -> rate limit
      -> authorization -> semantic validation -> audit -> handler
```

Claims dentro do payload nunca substituem `TrustedPeerContext`.

## Transporte e lifecycle

### Connect

Patch de baixa frequência em `ZNet.OnNewConnection`:

1. gerar connection id local;
2. registrar o RPC Majo no `peer.m_rpc`;
3. associar o peer à sessão;
4. cliente envia hello durante o handshake;
5. servidor valida e responde hello-ack.

### Login gate

Patch em `ZNet.RPC_PeerInfo` no servidor exige sessão Majo compatível. Ausência de hello, protocolo incompatível ou capability obrigatória ausente falha de forma fechada.

### Disconnect

Patch em `ZNet.Disconnect(ZNetPeer)` remove sessão, replay state, rate-limit state e mapping da conexão.

Não haverá pending async requests próprios nesta versão.

### Host/single-player

Operações locais futuras deverão entrar no mesmo gateway usando contexto trusted local/system criado server-side. Não haverá segundo pipeline de autorização.

## Protocol v1

- `MajoVersion`: 0.0.2;
- `ProtocolVersion`: 1;
- `ConfigSchema`: 0;
- `DataSchema`: 0.

Envelope binário:

```text
magic
wire-format version
message type
protocol version
operation id
request id
payload length
payload bytes
```

Regras:

- máximo total de 64 KiB;
- tamanho declarado deve casar exatamente com o buffer;
- sem `BinaryFormatter`, JSON obrigatório ou criptografia própria;
- response/error nunca contém stack trace;
- payload integral nunca é logado.

Tipos: `Hello`, `HelloAck`, `Request`, `Response`, `Error`.

## Handshake e capability

Capability obrigatória inicial: `Core.Network.v1`.

Hello contém somente versão pública, protocolo, execution side e capabilities limitadas. Compatibilidade inicial exige protocolo v1 e capability obrigatória. `MajoVersion` não é o contrato de rede.

## OperationRegistry

Cada operação registra:

- operation id numérico estável;
- direction: `ClientToServer`, `ServerToClient`, `ServerBroadcast`;
- allowed execution side;
- required permission;
- max payload;
- rate policy;
- audit policy;
- protocol version;
- validator;
- handler.

Não existe caminho `ClientToClient`.

## Permissions

Built-ins:

- `majo.none`;
- `majo.player`;
- `majo.admin`;
- `majo.system`.

O contrato aceita permission IDs específicas futuras. Grants são sempre construídos pelo servidor.

Admin remoto usa peer/socket/adminlist no servidor. `PlayerIsAdmin` não participa da autorização.

## Rate limiting

Fixed-window de baixa alocação por `ConnectionId + OperationId`, com janela, limite e burst. Estado é independente entre peers/operações e removido no disconnect.

Tráfego inválido antes de uma operação usa quota separada para evitar log/audit flooding.

## Replay

Cada sessão mantém uma janela limitada de request IDs. Repetição é rejeitada antes do handler. Reconnect cria nova connection id e nova janela.

Isto não substitui idempotência de domínio futura.

## Validation

Duas camadas:

1. estrutural: null, magic, tipo, protocolo, truncation, payload length e max size;
2. semântica por operação: ranges, enums, strings, collections, NaN/Infinity e IDs quando existirem payloads funcionais.

## Audit e erros

Códigos previsíveis: `Success`, `Unauthorized`, `Forbidden`, `InvalidPayload`, `UnsupportedProtocol`, `RateLimited`, `DuplicateRequest`, `UnknownOperation`, `HandlerFailed`, `Unavailable`.

Audit guarda somente contexto necessário, sem secrets/payload integral. Exceções de handler ficam no log interno; cliente recebe erro seguro.

## Threat model

| Ameaça | Mitigação | Evidência |
| --- | --- | --- |
| cliente malicioso/comprometido | bytes não confiáveis | gateway tests |
| sender/actor forjado | contexto vem do peer/RPC bound | spoofed actor test |
| admin forjado | admin calculado server-side | authorization test |
| payload truncado/malformado | codec fail-closed | codec tests |
| flood | quotas por conexão/operação | rate tests |
| payload oversized | limites global/operation | size tests |
| protocol mismatch | handshake rejeita | compatibility tests |
| reconnect abuse | cleanup + nova connection id | reconnect tests |
| handler exception | catch boundary | dispatcher test |
| replay/duplicate | request-id window | replay test |
| world mutation sem autorização | permission metadata obrigatória | auth tests |
| log flooding | quota de invalid traffic | invalid traffic test |
| enum/range/NaN/Infinity inválidos | validator semântico | validation tests |
| client-to-client privilegiado | direction não oferece C2C | registry tests |

## Patch ownership

Owner único: `Core.SecureRpcGateway`.

Superfícies previstas:

- `ZNet.OnNewConnection` — register/bind RPC;
- `ZNet.RPC_ClientHandshake` — envio do hello;
- `ZNet.RPC_PeerInfo` — compatibility gate;
- `ZNet.Disconnect` — cleanup.

São caminhos de connect/disconnect, não loops por frame.

Nenhum patch em `ZRoutedRpc` será necessário agora. A superfície continua reservada ao mesmo owner para futura necessidade.

## Threads

Core puro não toca Unity. Callbacks Valheim permanecem no pipeline do jogo. Nenhum GameObject, Player, Inventory ou ZDO mutável é manipulado off-thread.

## Testes

Harness puro:

- registry válido/duplicado/metadata/direction;
- session create/handshake/duplicate/disconnect/reconnect;
- player/admin/system/custom permissions;
- codec null/oversized/truncated/invalid type/protocol;
- rate normal/burst/exceeded/peers/operations/reset;
- replay;
- valid/unauthorized/unknown/handler exception dispatch;
- capability/protocol compatibility;
- spoofed actor/admin claims;
- invalid traffic quota.

Runtime/manual:

- binding real de ZNetPeer/ZRpc;
- adminlist real;
- connect/disconnect patches;
- mensagem visual de incompatibilidade.

Não falsificar integração Valheim no unit harness.

## CI

Preservar `foundation -> core-tests -> runtime-build`. O projeto de testes já inclui `Core/**/*.cs`.

## Dependências

Nenhuma nova dependência. BepInEx + Jötunn 2.30.1 permanecem únicas dependências de plataforma.

## Rollback

Reverter a implementação retorna a 0.0.1/ProtocolVersion 0. Não há migrations/save/persistência.

## Ready gate

- [x] estado fresco;
- [x] MAJO-001 DONE;
- [x] MAJO-000/001 lidas;
- [x] APIs relevantes revalidadas;
- [x] trust boundary;
- [x] threat model;
- [x] issue #10;
- [x] branch;
- [x] plano de testes/rollback.

Estado após o commit documental: READY.
