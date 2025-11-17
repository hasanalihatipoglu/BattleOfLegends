# Battle of Legends

A tactical turn-based strategy game built with C# and MonoGame.

## Overview

Battle of Legends is a hex-based tactical combat game featuring unit movement, combat resolution, and strategic card play. Players command armies in historical battles, managing morale, unit positioning, and tactical decisions.

## Features

- **Hex-based Movement System**: Units move across a hexagonal grid with pathfinding
- **Combat Mechanics**:
  - Melee and ranged combat
  - Dice-based attack and defense resolution
  - Retreat mechanics when units fail to defend
  - Unit advancement after successful attacks
- **Unit Types**: Infantry, Cavalry, Archers, Spears, Pikes, Leaders, and Light units
- **Card System**: Special abilities and tactical cards (Flanking, Envelopment, Cavalry Charge, etc.)
- **Turn-based Gameplay**: Strategic turn phases with action points and hand management
- **Morale System**: Track army morale throughout the battle

## Technology Stack

- **Language**: C# (.NET 9.0)
- **Framework**: MonoGame
- **Architecture**: Event-driven design with singleton managers

## Project Structure

```
BattleOfLegends/
├── BoLLogic/              # Core game logic
│   ├── Units/             # Unit classes and behaviors
│   ├── Tiles/             # Tile types and terrain
│   ├── Paths/             # Pathfinding and movement
│   ├── Cards/             # Card system
│   ├── Players/           # Player management
│   ├── Moves/             # Movement execution
│   ├── Attacks/           # Attack types
│   └── Scenarios/         # Game scenarios and save/load
├── Content/               # Game assets (sprites, sounds, fonts)
└── Game1.cs              # Main game loop
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- MonoGame framework

### Building

```bash
dotnet build
```

### Running

```bash
dotnet run
```

## Game Mechanics

### Combat Resolution

1. Attacker rolls dice based on unit health
2. Hits are determined by comparing rolls to attack points
3. Defender rolls dice equal to number of hits
4. Wounds are determined by comparing defense rolls to defense points
5. Retreats occur when defense roll exactly matches defense point
6. Units can advance into enemy positions after eliminating defenders

### Unit States

- Idle: Ready for orders
- Active: Selected and ready to move
- Moved/Marched: Has moved this turn
- Attacking/Defending: In combat
- Retreating/Retreated: Forced retreat after combat
- Advancing/Advanced: Moving into vacated enemy position

## Recent Fixes

- Fixed retreat resolution where units were announced to retreat but didn't actually move
- Added proper state management to prevent retreat cancellation
- Improved error handling in pathfinding for edge cases

## License

This project is licensed under the MIT License.

## Acknowledgments

Built with assistance from Claude Code.
