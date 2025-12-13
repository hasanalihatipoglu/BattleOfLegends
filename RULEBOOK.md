# BATTLE OF LEGENDS
## Official Rulebook

---

## TABLE OF CONTENTS
1. [Introduction](#introduction)
2. [Game Components](#game-components)
3. [Setup](#setup)
4. [How to Win](#how-to-win)
5. [Game Structure](#game-structure)
6. [Unit Types and Stats](#unit-types-and-stats)
7. [Movement](#movement)
8. [Combat](#combat)
9. [Cards](#cards)
10. [Terrain](#terrain)
11. [Special Abilities](#special-abilities)
12. [Quick Reference](#quick-reference)

---

## INTRODUCTION

Battle of Legends is a strategic hexagonal board game for 2 players, recreating ancient warfare between Rome and Carthage. Command units across the battlefield, play tactical cards at crucial moments, and outmaneuver your opponent to achieve victory.

**Players:** 2
**Factions:** Rome vs. Carthage
**Game Type:** Turn-based tactical combat

---

## GAME COMPONENTS

### Units
- 10 different unit types per faction
- Each unit has unique stats and abilities
- Units are categorized as:
  - **Light Units**: Can pass through occupied spaces
  - **Heavy Units**: Cannot pass through occupied spaces

### Cards
- 12 different tactical card types
- Each card activates during specific combat phases
- Cards can be discarded or reusable

### Resources
- **Morale Points**: Your army's fighting spirit
- **Hand Cards**: Limited tactical options
- **Action Points**: Resources for special actions

### Terrain
- **Grass**: Open ground (passable)
- **Water**: Blocks movement but not attacks
- **Hills**: Blocks both movement and attacks
- **Woods**: Variable terrain (implementation varies)

---

## SETUP

### Phase 1: Select Phase
1. Each player starts with their deck of tactical cards
2. Add cards from your deck to your hand (up to MaxHand limit)
3. You can remove cards from hand back to deck if needed
4. Select which units to deploy (scenario dependent)

### Phase 2: Order Phase
1. Triggered by playing the **Mixed Order** card
2. Select up to 3 units to receive special orders
3. These units are marked as "Ready" for coordinated action

### Phase 3: Turn Phase
- The main game begins
- Players alternate taking turns
- See **Game Structure** for detailed turn sequence

---

## HOW TO WIN

There are two paths to victory:

### 1. Morale Victory (Primary)
- Reduce your opponent's morale to 0
- **Gain morale**: +1 for each enemy heavy unit you destroy
- **Lose morale**: -1 when your heavy unit is destroyed
- **Important**: Light units do NOT affect morale when destroyed

### 2. Round Limit Victory
- If the game reaches the maximum number of rounds
- Player with higher morale wins
- If tied, specific scenario rules determine winner

---

## GAME STRUCTURE

### Round Structure

**Round Cycle** (4 rounds per cycle):
- **Rounds 1-2**: Initial player takes turns
- **Rounds 3-4**: Opponent takes turns
- Cycle repeats

**Round Start Effects**:
- On every odd round (1, 3, 5, 7...): All units reset to **Idle** state
- Action values reset to 0

### Turn Phases

Each turn consists of these phases in order:

1. **MOVE Phase**
   - Select and move your units
   - Choose between normal move or march

2. **ATTACK Phase**
   - Declare attacks with eligible units
   - Play attack-timing cards

3. **DEFEND Phase**
   - Defender responds to attack
   - Play defend-timing cards

4. **ROLL Phase**
   - Roll attack and defense dice
   - Play roll-timing cards (like Flanking)

5. **COUNTER Phase**
   - Defender may counter-attack
   - Play counter-timing cards

6. **ADVANCE Phase**
   - Winner may move into vacated space
   - Play advance-timing cards

7. **FORM Phase**
   - Reorganization phase (currently placeholder)

---

## UNIT TYPES AND STATS

### Unit Statistics Explained

- **Light**: Can pass through occupied tiles during movement
- **March**: Maximum distance for a full movement (cannot attack after)
- **Attack**: Maximum distance for attack movement (can attack after)
- **Range**: Maximum attack distance
- **Strength**: Health value (also determines attack dice pool)
- **Melee Attack**: Target number for melee attacks (roll ≥ this to hit)
- **Melee Defense**: Target number for melee defense (roll < this to take wound)
- **Ranged Attack**: Target number for ranged attacks
- **Ranged Defense**: Target number for ranged defense

### Complete Unit Table

| Unit | Class | Light | March | Attack | Range | Strength | MA | MD | RA | RD |
|------|-------|-------|-------|--------|-------|----------|----|----|----|----|
| **Leader** | Cavalry | Yes | 3 | 2 | 1 | 3 | 3 | 3 | 3 | 3 |
| **Archer** | Infantry | Yes | 2 | 0 | 3 | 2 | 4 | 4 | 5 | 4 |
| **Spear** | Infantry | Yes | 3 | 1 | 2 | 2 | 4 | 4 | 5 | 4 |
| **Pikes** | Infantry | No | 1 | 0 | 1 | 3 | 4 | 4 | 5 | 4 |
| **Infantry** | Infantry | No | 2 | 1 | 1 | 3 | 4 | 4 | 5 | 4 |
| **Cavalry** | Cavalry | No | 4 | 2 | 2 | 2 | 4 | 4 | 5 | 4 |
| **Numidians** | Cavalry | Yes | 4 | 2 | 2 | 2 | 4 | 4 | 5 | 4 |
| **Phoenicians** | Cavalry | No | 3 | 2 | 1 | 2 | 3 | 3 | 3 | 3 |
| **Equites** | Cavalry | No | 3 | 2 | 1 | 2 | 3 | 3 | 3 | 3 |
| **Velites** | Infantry | Yes | 3 | 1 | 2 | 2 | 5 | 5 | 5 | 5 |

*MA = Melee Attack, MD = Melee Defense, RA = Ranged Attack, RD = Ranged Defense*

---

## MOVEMENT

### Activating a Unit
1. Click on your unit to select it
2. Unit enters **Active** state
3. Reachable tiles are highlighted
4. Click destination to move

### Movement Types

#### Normal Move (Attack Move)
- Distance: Up to unit's **Attack** value
- After moving: Unit enters **Moved** state
- **Can still attack** this turn
- Use when positioning for combat

#### March (Full Move)
- Distance: Up to unit's **March** value (longer range)
- After moving: Unit enters **Marched** state
- **Cannot attack** this turn
- Use for repositioning or retreating

### Movement Rules

**Passable Terrain:**
- **Grass**: Always passable
- **Water**: NOT passable
- **Hills**: NOT passable
- **Woods**: (Varies by implementation)

**Occupied Tiles:**
- **Heavy Units**: Cannot move through occupied tiles
- **Light Units**: Can move through occupied tiles but cannot stop on them

**Special Movement:**
- **Retreat**: Forced movement away from attacker (automatic)
- **Advance**: Move into space vacated by destroyed enemy
- **Withdraw**: Escape combat using Withdraw card
- **Pursue**: Special cavalry follow-up movement
- **Hit and Run**: Cavalry disengagement movement

---

## COMBAT

### Declaring Combat

1. Select your unit (must be in **Moved** or **Active** state)
2. Click enemy unit within attack range
3. System validates:
   - Target is in range
   - Line of effect exists
   - Combat is legal
4. Combat type determined:
   - **Melee**: Attacker and defender are adjacent
   - **Ranged**: Target is beyond adjacent distance

### Combat Resolution Steps

#### Step 1: Roll Attack Dice
- Attacker rolls dice equal to their **Strength** (current health)
- Each die is a d6 (1-6)
- Modifiers from cards may add extra dice (e.g., Cavalry Charge +1)

#### Step 2: Determine Hits
- Compare each die to attacker's attack points:
  - **Melee Combat**: Use Melee Attack value
  - **Ranged Combat**: Use Ranged Attack value
- **Roll ≥ attack points = HIT**
- **Roll < attack points = MISS**

**Leadership Modifier (Melee Only):**
- For each friendly **Leader** adjacent to the attacker
- Reduce attack points by 1 (easier to hit)
- Example: Attack points 4, with 1 adjacent Leader = 3

#### Step 3: Defender Rolls Defense Dice
- Defender rolls one die per HIT received
- Each die is a d6 (1-6)

#### Step 4: Determine Wounds and Retreats
- Compare each defense die to defender's defense points:
  - **Melee Combat**: Use Melee Defense value
  - **Ranged Combat**: Use Ranged Defense value

**Die Results:**
- **Roll < defense points = WOUND** (reduce defender health by 1)
- **Roll = defense points = RETREAT** (forced movement)
- **Roll > defense points = BLOCKED** (no effect)

**Leadership Modifier (Retreats Only):**
- For each friendly **Leader** adjacent to the defender
- Reduce retreat count by 1
- **Leader units themselves cannot be forced to retreat**

#### Step 5: Apply Results

**If Defender Takes Wounds:**
- Reduce defender's Strength (health)
- If Strength reaches 0: Unit is **Dead**

**If Defender Must Retreat:**
- Unit enters **Retreating** state
- Automatically moves away from attacker
- Distance: `(MarchMove - 1) × Number of Retreats`
- Cannot retreat into impassable terrain or occupied tiles
- After retreat: Unit enters **Retreated** state

**If Defender Dies:**
- Unit removed from board
- **Morale Check**: If heavy unit:
  - Attacker faction: +1 morale
  - Defender faction: -1 morale
- If defending space is now empty: Attacker may **Advance**

### Counter-Attacks

During the **Counter Phase**, defender may counter-attack if:
- They have appropriate card (**Cavalry Counter**, **Counter**, **Skirmish**)
- Their unit has the Counter ability
- Unit is still alive and able to fight

Counter-attacks follow the same combat resolution steps above.

### Combat Modifiers Summary

| Modifier | Effect | When |
|----------|--------|------|
| Adjacent Leader (Attack) | -1 to attacker's attack points | Melee only |
| Adjacent Leader (Defense) | -1 to retreat count | Any combat |
| Cavalry Charge | +1 attack die | Attack Phase, distance ≥ 2 |
| Flanking | +1 to attack dice modifier | Roll Phase, friendly adjacent to target |
| Envelopment | +1 die per flanking unit | Roll Phase |
| First Strike | Defender attacks first | Defend Phase, infantry only |

---

## CARDS

### Card System Overview

**Hand Limit:**
- Each player has a maximum hand size (MaxHand)
- Cannot add cards beyond this limit
- Can remove cards back to deck to free space

**Card Timing:**
- Cards are only playable during their specific phase
- Cards automatically become available when conditions are met
- Wrong-phase cards cannot be played

**Card States:**
1. **In Deck**: Not available, in draw pool
2. **In Hand**: Selected and ready to use
3. **Ready to Play**: Available during correct phase
4. **Resolving**: Being played (displayed center screen)
5. **Discarded**: Used and removed (discard cards)
6. **Played**: Used and returned to hand (non-discard cards)

### Complete Card List

#### Attack Phase Cards

**Cavalry Charge** (Discard)
- **Target**: Cavalry units only
- **Requirement**: Attacker must move distance ≥ 2 before attacking
- **Effect**: Add +1 die to attack roll
- **Tactics**: Use when charging across open ground for devastating strikes

#### Defend Phase Cards

**Withdraw** (Discard)
- **Target**: Any unit
- **Effect**: Defending unit immediately moves to adjacent tile, escaping combat
- **Tactics**: Save valuable units from overwhelming attacks

**First Strike** (Discard)
- **Target**: Infantry units only
- **Effect**: Defender attacks first, before the main attack resolves
- **Tactics**: Turn defense into offense, potentially eliminating attacker

#### Roll Phase Cards

**Flanking** (Discard)
- **Target**: Any unit
- **Requirement**: At least one friendly unit adjacent to target
- **Effect**: Add +1 to attack dice modifier
- **Tactics**: Coordinate multiple units to surround enemies

**Envelopment** (Discard)
- **Target**: Any unit
- **Requirement**: Multiple friendly units adjacent to target
- **Effect**: Add +1 die modifier per flanking unit
- **Tactics**: Completely surround enemy for maximum damage

#### Counter Phase Cards

**Cavalry Counter** (Discard)
- **Target**: Cavalry units only
- **Effect**: Defending cavalry unit performs full counter-attack
- **Tactics**: Turn cavalry defense into offense

**Skirmish** (Discard)
- **Target**: Light cavalry units (Cavalry, Numidians)
- **Effect**: Light cavalry counter-attacks
- **Tactics**: Harass enemy with hit-and-run tactics

#### Advance Phase Cards

**Advance** (Non-Discard - Returns to Hand)
- **Target**: Any unit
- **Requirement**: Defender's tile is now vacant (unit died or retreated)
- **Effect**: Attacker moves into vacated space
- **Tactics**: Press advantage after successful melee combat
- **Special**: This card returns to hand after use

**Cavalry Pursue** (Discard)
- **Target**: Cavalry units only
- **Requirement**: After advancing
- **Effect**: Cavalry can move again and make another attack
- **Tactics**: Charge through enemy lines with relentless pursuit

**Hit and Run** (Discard)
- **Target**: Cavalry units only
- **Effect**: Cavalry can move after attacking without normal restrictions
- **Tactics**: Strike and disengage, avoid counter-attacks

#### Move Phase Cards

**Mixed Order** (Discard)
- **Target**: Any units
- **Effect**: Triggers Order Phase - select up to 3 units for coordinated action
- **Tactics**: Coordinate complex multi-unit maneuvers

### Card Strategy Tips

**Timing is Everything:**
- Save powerful cards for critical moments
- Don't waste discard cards on minor engagements

**Combo Potential:**
- Flanking + Envelopment = Massive damage
- Cavalry Charge + Cavalry Pursue = Double strike
- First Strike + Counter = Defensive domination

**Hand Management:**
- Balance aggressive and defensive cards
- Don't exceed hand limit
- Plan ahead for multi-turn strategies

---

## TERRAIN

### Terrain Effects Table

| Terrain | Movement | Attack Through | Special Rules |
|---------|----------|----------------|---------------|
| **Grass** | Passable | Yes | Default open terrain |
| **Water** | Blocked | Yes | Cannot move through but can attack across |
| **Hills** | Blocked | Blocked | Complete barrier for both movement and attacks |
| **Woods** | Variable | Variable | Implementation varies by scenario |

### Tactical Terrain Use

**Water:**
- Use as defensive barrier
- Ranged units can attack across water safely
- Forces enemies to find alternate routes

**Hills:**
- Creates completely impassable zones
- Blocks line of sight for ranged attacks
- Use to protect flanks and create choke points

**Grass:**
- Open battlefield
- Favors mobile units like cavalry
- Allows for flanking maneuvers

---

## SPECIAL ABILITIES

### Unit Abilities by Type

| Ability | Effect | Units |
|---------|--------|-------|
| **Withdraw** | Can use Withdraw card | Most units |
| **CavalryCharge** | Can use Cavalry Charge card | All cavalry |
| **CavalryCounter** | Can use Cavalry Counter card | All cavalry |
| **CavalryPursue** | Can use Cavalry Pursue card | All cavalry |
| **Flanking** | Can use Flanking card | Most units |
| **Advance** | Can use Advance card | Cavalry, Leader |
| **FirstStrike** | Can use First Strike card | Infantry |
| **HitAndRun** | Can use Hit and Run card | Light cavalry |
| **Skirmish** | Can use Skirmish card | Light units |

### Leader Special Rules

**Leadership Aura:**
- **Melee Attack Bonus**: Adjacent friendly units have -1 to attack points requirement (easier to hit)
- **Retreat Reduction**: Adjacent friendly units reduce retreat results by 1
- **Immune to Retreats**: Leaders themselves cannot be forced to retreat

**Strategic Value:**
- Place Leaders near frontline units for maximum benefit
- Protect your Leader - losing them removes valuable bonuses
- Target enemy Leaders to eliminate their bonuses

### Light Unit Rules

**Movement Advantages:**
- Can pass through occupied tiles (but cannot stop on them)
- Ideal for flanking maneuvers
- Can escape surrounded positions

**Combat Characteristics:**
- Do NOT affect morale when destroyed
- Generally have lower Strength (health)
- Excel at harassment and skirmishing

**Tactical Uses:**
- Screen your heavy units
- Flank enemy formations
- Sacrifice to protect valuable units

### Heavy Unit Rules

**Movement Restrictions:**
- Cannot pass through occupied tiles
- More restricted movement options

**Combat Characteristics:**
- Affect morale when destroyed (+1/-1)
- Generally higher Strength (health)
- Form the backbone of your army

**Tactical Uses:**
- Hold critical positions
- Anchor battle lines
- Absorb enemy charges

---

## QUICK REFERENCE

### Combat Quick Reference

1. **Declare**: Select attacker, target enemy in range
2. **Attack Roll**: Roll dice = attacker's Strength
3. **Hit Check**: Dice ≥ attack points = HIT
4. **Defense Roll**: Defender rolls dice = number of hits
5. **Wound Check**: Dice < defense points = WOUND, Dice = defense points = RETREAT
6. **Apply**: Reduce health, force retreat, check for death
7. **Morale**: If heavy unit dies, attacker +1, defender -1
8. **Counter**: Defender may counter-attack if able
9. **Advance**: Attacker may move into vacated space

### Phase Order Reference

1. **Move** → 2. **Attack** → 3. **Defend** → 4. **Roll** → 5. **Counter** → 6. **Advance** → 7. **Form**

### Unit State Reference

| State | Meaning | Can Move? | Can Attack? |
|-------|---------|-----------|-------------|
| **Idle** | Ready | Yes | Yes |
| **Active** | Selected | Yes | Yes |
| **Moved** | Has moved | No | Yes |
| **Marched** | Full move | No | No |
| **Attacked** | Has attacked | No | No |
| **Passive** | Turn complete | No | No |
| **Retreating** | Forced back | Automatic | No |
| **Advancing** | Moving forward | Automatic | No |

### Morale Quick Reference

**Gain Morale (+1):**
- Destroy enemy heavy unit

**Lose Morale (-1):**
- Your heavy unit is destroyed

**No Morale Effect:**
- Light unit destroyed (either side)

**Victory:**
- Reduce enemy morale to 0

### Common Mistakes to Avoid

1. **Don't march when you need to attack** - Marched units cannot attack
2. **Don't forget Leader bonuses** - Position Leaders near combat
3. **Don't waste heavy units** - They affect morale
4. **Don't exceed hand limit** - Manage your cards
5. **Don't ignore terrain** - Use water and hills strategically
6. **Don't forget counter-attacks** - Save Counter cards for critical moments
7. **Don't expose your Leader** - Losing them removes bonuses

---

## STRATEGY TIPS

### Opening Game
- Position heavy units in strong defensive positions
- Use light units to scout and screen
- Keep Leader units protected but near the front
- Build a balanced hand of cards

### Mid Game
- Look for flanking opportunities
- Use terrain to your advantage
- Coordinate multiple units with Mixed Order
- Save powerful cards for decisive moments

### Late Game
- Protect remaining heavy units (morale is critical)
- Press advantages aggressively
- Use Advance and Pursue to chain attacks
- Calculate morale changes carefully

### Advanced Tactics
- **The Hammer and Anvil**: Pin enemy with infantry, charge with cavalry
- **Refused Flank**: Defend one side, attack the other
- **Feigned Retreat**: Use light units to bait enemy into traps
- **Envelopment**: Surround enemy unit with multiple attackers
- **Rolling Attack**: Use Cavalry Pursue to attack multiple times
- **Defensive Counter**: First Strike + Counter for deadly defense

---

## GAME VARIANTS (Optional)

### Quick Game
- Lower starting morale (8 instead of 12)
- Smaller board
- Fewer units

### Epic Battle
- Higher starting morale (20+)
- Larger board
- More units and cards

### Scenario Play
- Historical battles with preset positions
- Special victory conditions
- Unique terrain layouts

---

## CREDITS

Battle of Legends is a strategic board game based on ancient warfare between Rome and Carthage.

**Game Design**: Based on codebase analysis
**Rulebook Compiled By**: Claude Code (AI Assistant)
**Date**: December 2025

---

## GLOSSARY

- **Adjacent**: Neighboring hexagon (directly touching)
- **Advance**: Move into space vacated by defeated enemy
- **Counter**: Defender's attack response
- **Discard**: Card removed from game after use
- **Flanking**: Attacking from multiple sides
- **Heavy Unit**: Unit that affects morale and cannot pass through occupied spaces
- **Light Unit**: Unit that can pass through occupied spaces and doesn't affect morale
- **March**: Full movement that prohibits attacking
- **Melee**: Adjacent combat
- **Morale**: Army's fighting spirit (victory resource)
- **Ranged**: Distant combat
- **Retreat**: Forced movement away from attacker
- **Strength**: Unit's health and attack dice pool

---

*May the gods of war favor your strategy!*
