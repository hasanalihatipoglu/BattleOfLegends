using System.Text.Json.Serialization;

namespace BoLLogic;

/// <summary>
/// Captures a complete snapshot of the game state at a specific point in time.
/// Used for robust undo/redo functionality.
/// </summary>
public class GameStateSnapshot
{
    // Board state
    public List<UnitSnapshot> Units { get; set; } = new();
    public List<CardSnapshot> Cards { get; set; } = new();
    public List<PlayerSnapshot> Players { get; set; } = new();

    // Turn state
    public PlayerType CurrentPlayer { get; set; }
    public PlayerType InitialPlayer { get; set; }
    public GamePhase GamePhase { get; set; }
    public TurnPhase TurnPhase { get; set; }
    public int GameRound { get; set; }

    // Combat state
    public CombatSnapshot Combat { get; set; }

    /// <summary>
    /// Creates a snapshot from the current board state
    /// </summary>
    public static GameStateSnapshot Capture(Board board)
    {
        var snapshot = new GameStateSnapshot
        {
            CurrentPlayer = board.CurrentPlayer,
            InitialPlayer = board.InitialPlayer,
            GamePhase = board.GamePhase,
            TurnPhase = board.TurnPhase,
            GameRound = board.GameRound
        };

        // Capture units
        foreach (var unit in board.Units)
        {
            snapshot.Units.Add(new UnitSnapshot
            {
                Type = unit.Type,
                Faction = unit.Faction,
                Position = new Position(unit.Position.Row, unit.Position.Column),
                Health = unit.Health.GetHealth(),
                State = unit.State,
                StateBeforeCombat = unit.StateBeforeCombat
            });
        }

        // Capture cards
        foreach (var card in board.Cards)
        {
            snapshot.Cards.Add(new CardSnapshot
            {
                Type = card.Type,
                Faction = card.Faction,
                State = card.State
            });
        }

        // Capture players
        foreach (var player in board.Players)
        {
            snapshot.Players.Add(new PlayerSnapshot
            {
                Type = player.Type,
                Morale = player.Morale.MoraleValue,
                Hand = player.Hand.HandValue,
                Action = player.Action.ActionValue
            });
        }

        // Capture combat state if combat is active
        var combatMgr = CombatManager.Instance;
        if (combatMgr.Attacker != null && combatMgr.Target != null)
        {
            snapshot.Combat = new CombatSnapshot
            {
                AttackerType = combatMgr.Attacker.Type,
                AttackerPosition = new Position(combatMgr.Attacker.Position.Row, combatMgr.Attacker.Position.Column),
                TargetType = combatMgr.Target.Type,
                TargetPosition = new Position(combatMgr.Target.Position.Row, combatMgr.Target.Position.Column),
                Type = combatMgr.Type,
                DiceModifier = combatMgr.DiceModifier
            };
        }

        return snapshot;
    }

    /// <summary>
    /// Restores the board to this snapshot's state
    /// </summary>
    public void Restore(Board board)
    {
        // Restore turn state
        board.CurrentPlayer = CurrentPlayer;
        board.InitialPlayer = InitialPlayer;
        board.GamePhase = GamePhase;
        board.TurnPhase = TurnPhase;
        board.GameRound = GameRound;

        TurnManager.Instance.CurrentPlayer = CurrentPlayer;
        TurnManager.Instance.CurrentTurn = CurrentPlayer;
        TurnManager.Instance.CurrentGamePhase = GamePhase;
        TurnManager.Instance.CurrentTurnPhase = TurnPhase;
        TurnManager.Instance.CurrentGameRound = GameRound;

        // First, clear all tile occupancy to start fresh
        foreach (var tile in board.Tiles)
        {
            tile.Unit = null;
            tile.Occupied = false;
        }

        // Restore units to their snapshot positions
        foreach (var unitSnapshot in Units)
        {
            var unit = board.Units.FirstOrDefault(u =>
                u.Type == unitSnapshot.Type && u.Faction == unitSnapshot.Faction);

            if (unit != null)
            {
                // Find target tile
                var targetTile = board.Tiles.FirstOrDefault(t =>
                    t.Position.Row == unitSnapshot.Position.Row &&
                    t.Position.Column == unitSnapshot.Position.Column);

                if (targetTile != null)
                {
                    // Place unit at target position
                    targetTile.Unit = unit;
                    targetTile.Occupied = true;
                    unit.Tile = targetTile;
                    unit.Position = targetTile.Position;
                }

                // Restore unit state
                unit.Health.SetHealth(unitSnapshot.Health);
                unit.State = unitSnapshot.State;
                unit.StateBeforeCombat = unitSnapshot.StateBeforeCombat;
            }
        }

        // Restore cards
        for (int i = 0; i < Cards.Count && i < board.Cards.Count; i++)
        {
            var cardSnapshot = Cards[i];
            var card = board.Cards.FirstOrDefault(c =>
                c.Type == cardSnapshot.Type && c.Faction == cardSnapshot.Faction);

            if (card != null)
            {
                card.State = cardSnapshot.State;
            }
        }

        // Restore players
        foreach (var playerSnapshot in Players)
        {
            var player = board.Players.FirstOrDefault(p => p.Type == playerSnapshot.Type);
            if (player != null)
            {
                player.Morale.MoraleValue = playerSnapshot.Morale;
                player.Hand.HandValue = playerSnapshot.Hand;
                player.Action.ActionValue = playerSnapshot.Action;
            }
        }

        // Always clear combat state during undo/redo
        // Combat is a transient UI state, not part of the core game state
        // Users should not be able to interact with combat UI during undo/redo
        var combatMgr = CombatManager.Instance;
        combatMgr.Attacker = null;
        combatMgr.Target = null;
        combatMgr.DiceModifier = 0;
    }
}

/// <summary>
/// Snapshot of a unit's state
/// </summary>
public class UnitSnapshot
{
    public UnitType Type { get; set; }
    public PlayerType Faction { get; set; }
    public Position Position { get; set; }
    public int Health { get; set; }
    public UnitState State { get; set; }
    public UnitState StateBeforeCombat { get; set; }
}

/// <summary>
/// Snapshot of a card's state
/// </summary>
public class CardSnapshot
{
    public CardType Type { get; set; }
    public PlayerType Faction { get; set; }
    public CardState State { get; set; }
}

/// <summary>
/// Snapshot of a player's state
/// </summary>
public class PlayerSnapshot
{
    public PlayerType Type { get; set; }
    public int Morale { get; set; }
    public int Hand { get; set; }
    public int Action { get; set; }
}

/// <summary>
/// Snapshot of combat state
/// </summary>
public class CombatSnapshot
{
    public UnitType AttackerType { get; set; }
    public Position AttackerPosition { get; set; }
    public UnitType TargetType { get; set; }
    public Position TargetPosition { get; set; }
    public AttackType Type { get; set; }
    public int DiceModifier { get; set; }
}
