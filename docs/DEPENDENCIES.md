# Dependências

## Princípio

O MajoServerModpack evita depender de mods funcionais de terceiros. A regra é possuir a própria implementação e depender apenas de frameworks cuja função seja fornecer infraestrutura de modding.

Uma dependência nova exige:

1. necessidade arquitetural clara;
2. análise de licença;
3. risco de supply chain;
4. custo de compatibilidade;
5. impacto de runtime/performance;
6. decisão documentada.

## Dependências de plataforma aprovadas

### BepInEx

**Papel**

- bootstrap do plugin;
- loader;
- lifecycle base;
- logging base;
- Harmony/patching disponível na plataforma.

**Por que permanece dependência**

Reimplementar loader/injeção/patch runtime seria custo alto, risco técnico e não é diferencial do produto.

### Jötunn

**Papel**

- framework específico de Valheim;
- adaptação de plumbing recorrente da API do jogo;
- utilitários/versionamento do jogo;
- serviços de integração que podem reduzir quebra após updates.

**Por que permanece dependência**

Valheim não oferece uma API oficial de mods estável. Jötunn concentra parte importante da compatibilidade com mudanças do jogo.

**Limite**

O Majo não deve delegar a Jötunn:

- autorização privilegiada;
- modelo de configuração do produto;
- domínio de gameplay;
- persistência própria;
- decisões de compatibilidade do produto.

Esses contratos pertencem ao Majo.

## Baseline MAJO-001

Combinação candidata revalidada em 2026-09-18:

| Componente | Versão | Papel |
| --- | --- | --- |
| Valheim | 1.0.15 | jogo/runtime alvo |
| BepInExPack_Valheim | 5.4.2350 | distribuição Valheim do loader |
| BepInEx | 5.4.23.5 | framework/loader efetivo |
| Jötunn | 2.30.1 | framework Valheim |
| JotunnLib | 2.30.1 | referência de build |
| Microsoft.NETFramework.ReferenceAssemblies.net462 | 1.0.3 | build-only, PrivateAssets=All |

A tabela não significa auto-update. Promoção depende dos gates da tarefa e futura matriz de compatibilidade.

O artefato Linux x64 de BepInEx 5.4.23.5 usado pelo CI possui SHA-256 oficial:

`e538560be65739f562519ab518a75f9c65b3f57f87457403ae7cde683c12dab7`

## O que não adicionar por padrão

- ServerSync;
- DI framework;
- logging framework;
- networking library;
- UI framework externo;
- Newtonsoft adicional;
- mods funcionais;
- bibliotecas apenas por conveniência.

## Supply chain

- versões promovidas manualmente;
- origem oficial/upstream;
- hash de artefato quando aplicável;
- nada de auto-download/update em runtime;
- DLLs proprietárias do Valheim não entram no Git;
- falha de compatibilidade deve ser observável.

## Referências funcionais

Projetos em `docs/REFERENCE-PROJECTS.md` são referências de comportamento/arquitetura e não dependências do produto.


## Dependências do launcher

A política de dependências do runtime Valheim e do launcher são separadas.

- BepInEx/Jötunn são dependências do runtime Valheim, não do processo do launcher.
- O launcher poderá usar componentes desktop/.NET necessários à UI, mas novas dependências passam pela mesma análise de licença, supply chain e manutenção.
- A tecnologia de UI desktop será decidida na MAJO-004.
- O launcher não deve baixar/executar binários arbitrários.
- Qualquer futuro instalador/updater exige origem confiável, manifest/hashes, staging e rollback em tarefa específica.
