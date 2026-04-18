# RPG 3D Unity Prototype

Este repositório contém um protótipo de RPG 3D em Unity.

## Abrir no Unity Hub (Unity 6000.3.13f1)

1. Clone o repositório.
2. No Unity Hub, instale/selecione a versão **6000.3.13f1**.
3. Clique em **Add** e selecione a pasta raiz do projeto:
   - pasta que contém **`Assets/`**, **`Packages/`** e **`ProjectSettings/`**.
4. Abra o projeto pelo Unity Hub.

### Pasta correta para adicionar no Hub

Neste repositório, a pasta correta é a raiz clonada (por exemplo: `.../teste`).

Não selecione a subpasta `teste/` interna (ela contém apenas um README legado e não é raiz de projeto Unity).

## Estrutura mínima de projeto Unity incluída

- `Assets/`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `ProjectSettings/ProjectVersion.txt`

## Arquivos gerados automaticamente no primeiro open

No primeiro open, o Unity pode gerar arquivos adicionais em `ProjectSettings/`, como:

- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/TagManager.asset`

Isso é esperado para um projeto mínimo recém-estruturado.

## Controles (protótipo)

- **WASD**: mover
- **Shift**: correr
- **Espaço**: pular
- **Mouse esquerdo**: ataque melee
- **Mouse direito**: magia
- **V**: alternar 1ª/3ª pessoa
- **I**: inventário
- **J**: quest log
- **Esc**: pausa / save-load
