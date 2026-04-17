# RPG 3D Unity Prototype

Este repositório agora contém um **protótipo jogável** de RPG 3D em Unity (runtime-generated), com:

- personagem controlável (WASD, corrida, pulo, câmera 1ª/3ª pessoa)
- combate melee + magia com cooldown e crítico
- 5 tipos de inimigos (Goblin, Orc, Skeleton, Dragon, Bandit)
- 4 áreas exploráveis (Village, Forest, Dungeon, Castle Entrance)
- sistema de XP/nível/atributos
- inventário com 24 slots, catálogo de 30 equipamentos + 50 consumíveis
- 15 quests em cadeia com progresso
- NPCs (8), baús, portas interativas
- HUD, inventário, quest log, pausa e save/load em 3 slots
- ciclo dia/noite e clima simples

## Estrutura de scripts (MVP completo)

Além do bootstrap único (`Assets/Scripts/RPGPrototypeBootstrap.cs`), o projeto agora inclui estrutura modular com os sistemas pedidos:

- `Assets/Scripts/Player/` (`PlayerController`, `PlayerStats`, `PlayerCombat`, `PlayerInventory`)
- `Assets/Scripts/Enemy/` (`EnemyBase`, `GoblinAI`, `OrcAI`, `SkeletonAI`, `DragonAI`, `BossAI`)
- `Assets/Scripts/Quest/` (`QuestManager`, `Quest`)
- `Assets/Scripts/Items/` (`Item`, `Equipment`, `Consumable`)
- `Assets/Scripts/NPC/` (`NPCBase`)
- `Assets/Scripts/UI/` (`HUDManager`, `InventoryUI`, `CharacterSheetUI`, `QuestLogUI`, `DialogueUI`, `MenuUI`, `ShopUI`)
- `Assets/Scripts/Manager/` (`SaveManager`, `AudioManager`)

## Como executar

1. Instale **Unity 2021.3+**.
2. Abra este diretório no Unity Hub.
3. Crie/abra qualquer cena vazia e pressione **Play**.
   - O bootstrap (`Assets/Scripts/RPGPrototypeBootstrap.cs`) cria todo o protótipo em runtime.

## Controles

- **WASD**: mover
- **Shift**: correr
- **Espaço**: pular
- **Mouse esquerdo**: ataque melee
- **Mouse direito**: magia
- **V**: alternar 1ª/3ª pessoa
- **I**: inventário
- **J**: quest log
- **Esc**: pausa / save-load
