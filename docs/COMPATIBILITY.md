# Compatibilidade

## Objetivo

Permitir evolução contínua sem transformar toda atualização do Majo em incompatibilidade de rede.

## Dimensões independentes

- `MajoVersion`: versão pública do produto.
- `ProtocolVersion`: wire contract/RPC.
- `ConfigSchema`: configuração persistida/sincronizada.
- `DataSchema`: dados próprios persistidos.
- capabilities: recursos negociáveis quando necessários.

## Lado de execução

Cada módulo deve declarar:

| Tipo | Significado |
| --- | --- |
| ClientOnly | somente apresentação/input local |
| ServerOnly | somente servidor; cliente não precisa executar lógica equivalente |
| ClientAndServer | ambos os lados necessários para semântica correta |
| OptionalClient | servidor funciona com cliente ausente e existe degradação segura explicitamente testada |

A classificação pertence ao módulo, não ao framework usado.

## Política inicial

Durante a fundação, adotar postura conservadora:

- features de gameplay compartilhado: exigir peer Majo compatível;
- config server-authoritative: cliente deve conseguir receber/aplicar o contrato correspondente;
- features puramente server-only podem permanecer server-only;
- features puramente locais não devem obrigar sincronização sem motivo.

A MAJO-002 definirá o handshake concreto e os códigos de rejeição/degradação.

## Compatibilidade de versão

Não usar simplesmente `clientVersion == serverVersion`.

Exemplo válido no futuro:

```text
MajoVersion:      0.4.8
ProtocolVersion:  3
ConfigSchema:     5
DataSchema:       2
Capabilities:
  Inventory: 2
  Map: 1
```

Um patch visual pode alterar `MajoVersion` sem alterar `ProtocolVersion`.

## Frameworks

BepInEx/Jötunn são versões promovidas e testadas, nunca auto-updated pelo Majo.

Matriz em construção:

| Majo | Valheim | BepInEx | Jötunn | Estado |
| --- | --- | --- | --- | --- |
| 0.0.1 | 1.0.15 | 5.4.23.5 (BepInExPack 5.4.2350) | 2.30.1 | candidata — build PASS; smoke runtime pendente |

A promoção para suportada exige evidência correspondente; build isolado não equivale a smoke dentro do Valheim.

## Contexto de execução

A MAJO-001 diferencia dedicated, listen host, client, local world e menu/pre-world quando o runtime oferece sinal verificável. Falta de sinal degrada para estado conservador e observável, nunca para uma inferência de autoridade.

## Falha segura

Quando capability/protocolo requerido estiver ausente:

- rejeitar conexão com mensagem clara se a semântica compartilhada ficaria incorreta; ou
- desabilitar somente a feature quando a degradação segura estiver prevista e testada.

Nunca continuar silenciosamente com estado de gameplay divergente.
