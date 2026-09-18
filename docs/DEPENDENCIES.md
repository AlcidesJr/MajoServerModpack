# Dependências

## Política

O MajoServerModpack aceita dependências externas somente quando representam **plataforma/framework** e reduzem trabalho estrutural que seria continuamente afetado por atualizações do Valheim.

Mods funcionais de terceiros não são dependências.

## Dependências aprovadas

### BepInEx

Papel:
- bootstrap/loader;
- plugin lifecycle;
- logging/config base;
- Harmony/patch infrastructure.

### Jötunn

Papel:
- abstrações específicas de Valheim;
- lifecycle de conteúdo;
- serviços de modding que absorvem parte das mudanças internas do jogo;
- transporte/sincronização quando adequado.

## Baseline inicial

A combinação exata suportada será registrada em uma matriz de compatibilidade assim que existir build executável. Até lá:

- BepInEx 5.x/LTS para Valheim é a direção arquitetural;
- Jötunn 2.x é a direção arquitetural;
- nenhuma versão será atualizada automaticamente.

## Processo de atualização

Nova versão de Valheim, BepInEx ou Jötunn deve gerar avaliação explícita:

1. ler changelog/diff relevante;
2. comparar APIs consumidas por `Majo.Platform`;
3. build;
4. testes focados;
5. dedicated server;
6. cliente;
7. multiplayer;
8. regressões de config/RPC/persistência;
9. performance quando hot paths forem afetados;
10. promover somente após evidência.

## Regra de isolamento

Módulos devem depender de interfaces próprias quando isso protege o domínio de alterações externas.

Exemplo desejado:

```text
Building → IPieceRegistry → JotunnAdapter → Jötunn
```

Evitar espalhar `PieceManager.Instance`, `PrefabManager.Instance` e equivalentes por módulos sem necessidade.

## Nova dependência

Qualquer proposta de nova dependência de runtime deve responder:

- por que BepInEx/Jötunn/.NET/Valheim não bastam;
- qual custo de atualização ela adiciona;
- qual superfície de supply chain cria;
- se pode ser isolada;
- plano de substituição/rollback.

Sem decisão versionada, a dependência não entra.
