# PLAN — MAJO-000

## Resumo da solução

Materializar a governança e arquitetura aprovadas, depois aprofundar o catálogo de features/referências e suas sobreposições antes de encerrar a tarefa.

## Estado atual

O repositório possuía somente o bootstrap mínimo. Não havia arquitetura, Board, política de dependências ou contratos de desenvolvimento.

## Superfícies afetadas

- documentação raiz;
- `docs/`;
- `work/`;
- governança.

## Contratos e compatibilidade

- nenhuma API de runtime existe ainda;
- nenhuma feature de gameplay pode ser implementada nesta tarefa;
- BepInEx/Jötunn são plataforma, não domínio;
- mods de referência não entram como dependências.

## Passos de implementação

1. bootstrap mínimo do repositório;
2. criar issue/branch MAJO-000;
3. registrar governança e arquitetura;
4. registrar dependências/versionamento;
5. registrar conflitos e referências;
6. aprofundar catálogo funcional por referência;
7. revisar coerência da arquitetura contra o catálogo;
8. review independente;
9. security review;
10. PR/merge/closeout.

## Estratégia de testes

1. validar presença/coerência dos documentos;
2. revisar links internos e estados;
3. confirmar ausência de código/runtime indevido no diff;
4. review independente do contrato arquitetural.

## Segurança

- Superfície relevante: sim, pois a arquitetura define futura fronteira de confiança.
- Revisões necessárias: identidade de peer, autorização, RPC, configuração server-authoritative e supply chain.

## Dados e migrations

N/A.

## Paralelismo

- Pode paralelizar: parcialmente durante catalogação de referências.
- Contrato: cada análise entrega features, patches, rede, persistência, overlap, riscos e recomendação Majo.
- Ownership: integração final permanece na MAJO-000.

## Rollback

Reverter o commit/PR documental; nenhum save/runtime é afetado.

## Riscos

- abstração prematura → manter contratos conceituais e adiar interfaces concretas para tarefas que possuam casos reais;
- dependência excessiva → Majo.Platform;
- conflito futuro invisível → matriz obrigatória por feature;
- segurança delegada ao framework → SecureRpcGateway próprio.

## Critério para iniciar implementação

- [x] Escopo compreendido.
- [x] Dependências satisfeitas.
- [x] Contratos relevantes identificados.
- [x] Estratégia de teste definida.
- [x] Riscos materiais tratados.

Estado autorizado: IMPLEMENTING, somente documentação/governança.
