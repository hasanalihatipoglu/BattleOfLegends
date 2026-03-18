# Battle of Legends — Software Architecture

```mermaid
flowchart TB
    %% ─── Entry & Persistence ──────────────────────────────────────────────
    subgraph DATA["💾 Data & Persistence"]
        JSON["JSON Scenario Files\n(unit placements, cards, board)"]
        SAVE["Save / Load\n(GameData / GameStateData)"]
    end

    %% ─── Core Singletons ───────────────────────────────────────────────────
    subgraph CORE["⚙️ Core Managers (Singletons)"]
        GM["GameManager\n• Loads scenario JSON\n• Holds CurrentBoard\n• Bootstrap entry point"]
        TM["TurnManager\n• Phase state machine\n• Player switching\n• Turn / Round counters\n• Fires change events"]
        HM["HistoryManager\n• Undo / Redo stacks\n• Turn-boundary snapshots\n• JSON serialisation"]
        MC["MessageController\n• In-game messages / dialogs"]
        SC["SoundController\n• SFX playback"]
    end

    %% ─── Board ─────────────────────────────────────────────────────────────
    subgraph BOARD["🗺️ Board"]
        BRD["Board\n• Units[ ]\n• Tiles[ ]\n• Cards[ ]\n• Players[ ]\n• Positions grid\n• Reflects types from JSON"]
        GS["GameState\n• CurrentPlayer\n• TurnPhase\n• GamePhase\n• GameRound"]
    end

    %% ─── Tile System ────────────────────────────────────────────────────────
    subgraph TILES["🏔️ Tiles"]
        TILE["Tile (abstract)\n• Position (row, col)\n• Passable / CanBeAttackPassed\n• Occupant unit\n• Adjacent tiles list"]
        GRASS["Grass"]
        HILL["Hill"]
        WATER["Water"]
        TILE --> GRASS & HILL & WATER
    end

    %% ─── Unit System ────────────────────────────────────────────────────────
    subgraph UNITS["⚔️ Unit System"]
        UNIT["Unit (abstract)\n• MarchMove / AttackMove\n• Melee & Ranged stats\n• Skills & Abilities\n• State machine"]
        HSY["HealthSystem\n• HP tracking\n• Damage / Heal / Death"]
        USV["UnitStateValidator\n• Guards invalid\n  state transitions"]
        UNIT --> HSY & USV

        INF["Infantry"]
        ARC["Archer"]
        CAV["Cavalry"]
        SPR["Spear"]
        EQU["Equites"]
        NUM["Numidians"]
        PHO["Phoenicians"]
        VEL["Velites"]
        LDR["Leader\n(NO_RETREAT skill)"]
        UNIT --> INF & ARC & CAV & SPR & EQU & NUM & PHO & VEL & LDR
    end

    %% ─── Player System ──────────────────────────────────────────────────────
    subgraph PLAYERS["👥 Player System"]
        PLY["Player\n• Faction (Rome / Carthage)\n• Leader reference"]
        MOR["MoraleSystem\n• Morale value 0-10\n• Win condition check"]
        HND["HandSystem\n• Hand-size constraint\n• Drift correction"]
        ACT["ActionSystem\n• Action points spent\n• Max action limit"]
        PLY --> MOR & HND & ACT
    end

    %% ─── Card System ────────────────────────────────────────────────────────
    subgraph CARDS["🃏 Card System"]
        CARD["Card (abstract)\n• Timing (TurnPhase)\n• Target (UnitClass)\n• CardClass (Order / Special / Ability)\n• IsValid() / Play()"]
        CORD["Order Cards\nMixedOrder"]
        CSPC["Special Cards\nAdvance · CavalryCharge\nCavalryCounter · CavalryPursue\nEnvelopment · FirstStrike\nFlanking · HitAndRun\nSkirmish · Withdraw"]
        CARD --> CORD & CSPC
    end

    %% ─── Combat System ──────────────────────────────────────────────────────
    subgraph COMBAT["🎲 Combat System"]
        CM["CombatManager\n• Dice rolls (1-6)\n• Hit / Wound / Retreat calc\n• Leadership modifiers\n• Melee vs Ranged detection"]
        NA["NormalAttack"]
        CA["CounterAttack"]
        FA["FirstAttack"]
        CM --> NA & CA & FA
    end

    %% ─── Order System ───────────────────────────────────────────────────────
    subgraph ORDERS["📋 Order System"]
        OM["OrderManager\n• GiveOrder(faction, type)\n• OrderLimit tracking\n• Validates ordered units"]
    end

    %% ─── Pathfinding ────────────────────────────────────────────────────────
    subgraph PATHS["🧭 Pathfinding"]
        PF["PathFinder (BFS)\n• Move / Attack / Retreat paths\n• Frontiers (boundary tiles)\n• Engagement detection\n• Friendly support check"]
        PTH["Path\n(tile sequence)"]
        FRT["Frontier\n(boundary tile set)"]
        DIR["Direction\n(6-way hex offsets)"]
        RGN["Region / RegionBuilder"]
        PF --> PTH & FRT & DIR & RGN
    end

    %% ─── History Actions ────────────────────────────────────────────────────
    subgraph HIST["📜 History Actions"]
        GA["GameAction (abstract)\n• Execute() / Undo()"]
        ACTS["UnitMoveAction\nCombatAction\nEndTurnAction\nCardPlayAction\nPlayerChangeAction\nMoraleChangeAction\nActionValueChangeAction\nPhaseChangeAction\nGamePhaseChangeAction\nGameRoundChangeAction\nRoundResetAction"]
        SNAP["GameStateSnapshot\n(full board capture)"]
        GA --> ACTS
        HM --> SNAP & GA
    end

    %% ─── Game Phase / Turn Phase Flow ───────────────────────────────────────
    subgraph PHASES["🔄 Phase State Machine"]
        direction LR
        GP1["Select"] --> GP2["Order"] --> GP3["Turn"] --> GP4["End"]
        TP1["Move"] --> TP2["Attack"] --> TP3["Defend"] --> TP4["Roll"]
        TP4 --> TP5["Counter"] --> TP6["Advance"] --> TP7["Form"]
    end

    %% ─── Unit State Machine ─────────────────────────────────────────────────
    subgraph USTATE["🔁 Unit State Machine"]
        direction LR
        US1["Idle"] --> US2["Ready"] --> US3["Active"]
        US3 --> US4["Moved"] & US5["Marched"] & US6["Attacked"]
        US4 & US5 & US6 --> US7["Retreating / Advancing"]
        US7 --> US8["Passive / Dead / Ordered"]
    end

    %% ─── Card State Flow ────────────────────────────────────────────────────
    subgraph CSTATE["🃏 Card State Flow"]
        direction LR
        CS1["InDeck"] -->|draw| CS2["InHand"]
        CS2 -->|phase match| CS3["ReadyToPlay"]
        CS3 -->|click| CS4["Resolving"]
        CS4 -->|effect| CS5["Discarded"]
        CS4 -->|cancel| CS2
    end

    %% ─── Primary Data Flow ──────────────────────────────────────────────────
    JSON -->|parse| GM
    GM -->|creates| BRD
    GM -->|saves/loads via| SAVE
    BRD --> GS
    BRD --> TILE
    BRD --> UNIT
    BRD --> CARD
    BRD --> PLY

    GM --> TM
    TM -->|phase events| CARD
    TM -->|phase events| COMBAT
    TM -->|turn boundary| HM

    CARD -->|Order card| OM
    OM -->|activates| UNIT
    CARD -->|Combat card| COMBAT
    CARD -->|Move card| PF

    COMBAT -->|uses paths| PF
    COMBAT -->|damages| HSY
    COMBAT -->|records| HM
    COMBAT -->|morale impact| MOR

    ACT -->|limits| UNIT
    HND -->|limits draws| CARD

    PF -->|traverses| TILE

    HM -->|snapshot| GS
    HM -->|undo applies| BRD
```

---

## Layer Summary

| Layer | Responsibility |
|-------|---------------|
| **Data / Persistence** | JSON scenarios, save/load serialisation |
| **Core Managers** | GameManager, TurnManager, HistoryManager, MessageController, SoundController |
| **Board** | Central model — owns all game objects |
| **Tile System** | Hex grid, passability, adjacency |
| **Unit System** | Stats, health, state machine, abilities |
| **Player System** | Morale, hand size, action points |
| **Card System** | Timing, validation, play effects |
| **Combat System** | Dice, hit/wound/retreat resolution |
| **Order System** | Leader order giving and limits |
| **Pathfinding** | BFS hex movement, attack/retreat paths |
| **History** | Undo/redo via snapshot + command pattern |

## Key Design Patterns

- **Singleton** — GameManager, TurnManager, CombatManager, OrderManager, PathFinder, HistoryManager
- **Abstract Factory** — Unit/Card/Tile types instantiated via reflection from JSON names
- **State Machine** — TurnPhase, GamePhase, UnitState, CardState all modelled as explicit state machines
- **Snapshot + Command** — HistoryManager stores full-board snapshots at turn boundaries alongside reversible GameAction objects
- **Event-Driven** — TurnManager fires events on every phase/player/hand/action change; Board components subscribe
- **Hexagonal Grid** — Even/odd row offset coordinate system with 6-directional movement via Direction enum
```
