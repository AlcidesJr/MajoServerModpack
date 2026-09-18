# Security baseline

## Objetivo

Definir propriedades mínimas de segurança antes que módulos concretos criem rede, persistência, administração ou serviços externos.

## Trust boundaries

### Cliente ↔ servidor Valheim

O cliente é não confiável para qualquer decisão privilegiada.

Obrigatório para requests privilegiados:

1. identidade vinculada ao peer/socket real;
2. autorização server-side;
3. validação estrutural e semântica do payload;
4. limites de tamanho/faixa;
5. rate limiting quando houver potencial de abuso;
6. auditabilidade da ação;
7. execução somente após todas as validações.

Um estado de UI como “admin” nunca é autorização.

### Framework ↔ Majo

BepInEx/Jötunn são dependências de plataforma, mas não fronteiras de confiança para regras administrativas. APIs de transporte podem ser utilizadas sem delegar a elas a decisão final de autorização do produto.

## Supply chain

- versões de framework são fixadas e promovidas manualmente;
- registrar origem oficial, versão e SHA-256 do artefato testado quando aplicável;
- não fazer auto-update/download de framework em runtime;
- mudança de framework exige tarefa/gate de compatibilidade;
- binários de referência/mods funcionais não entram no produto apenas por estarem disponíveis.

## RPC e protocolo

A MAJO-002 deve definir:

- envelope/versionamento;
- tamanho máximo;
- allowlist de operações;
- autorização por operação;
- erro seguro sem revelar dados sensíveis;
- comportamento para replay/duplicação quando a operação não for naturalmente idempotente;
- timeout/limites de fila;
- logging rate-limited para payload inválido.

Operações destrutivas devem receber proteções adicionais adequadas ao domínio.

## Persistência

Para dados próprios:

- namespace Majo;
- schema explícito;
- migrations testáveis;
- nunca descartar item/dado silenciosamente por mudança de schema;
- escrita preferencialmente atômica quando filesystem estiver envolvido;
- import paths confinados ao diretório permitido;
- backup/restore valida origem, path e formato antes de substituir dados.

## HTTP/WebSocket/WebMap

Qualquer serviço externo futuro:

- desabilitado por padrão;
- bind em loopback por padrão quando possível;
- LAN/WAN somente por opt-in;
- endpoints mutáveis ou administrativos exigem autenticação/autorização;
- CORS/origin/header não substituem autenticação;
- rate limit e payload limits;
- proteção contra path traversal;
- não expor world seed, dados privados, inventários ou informações administrativas sem política explícita;
- TLS pode ser delegado a reverse proxy, mas essa topologia deve ser documentada;
- thread web nunca toca estado Unity mutável diretamente: usar snapshots/queues.

## Administração

Ações como spawn, give, teleport, kick, ban, world mutation, restore e bulk-delete passam pelo SecureRpcGateway.

Audit mínimo futuro:

- timestamp;
- actor/peer;
- operation;
- target;
- resultado;
- campos de contexto necessários sem registrar secrets.

## Logging e segredos

Não registrar:

- tokens;
- passwords;
- session secrets;
- conteúdo de credenciais.

Dados de jogador devem ser mínimos e proporcionais ao diagnóstico/operação.

## Fail-safe

Quando compatibilidade, identidade ou autorização não puderem ser provadas:

- negar a operação privilegiada; ou
- desabilitar a feature afetada quando isso for seguro.

Nunca degradar silenciosamente para “permitir”.
