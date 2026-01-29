namespace BoLLogic;

/// <summary>
/// Represents a unit movement action that can be undone/redone
/// </summary>
public class UnitMoveAction : GameAction
{
    public UnitType UnitType { get; set; }
    public Position FromPosition { get; set; }
    public Position ToPosition { get; set; }
    public UnitState PreviousState { get; set; }
    public UnitState NewState { get; set; }
    public int PreviousHealth { get; set; }
    public int NewHealth { get; set; }

    // Track units that were locked by this move (for undo/redo)
    // Key: "Faction-UnitType" to uniquely identify units
    // Value: Previous state before locking (Idle or Ready)
    public Dictionary<string, UnitState> LockedUnits { get; set; } = new Dictionary<string, UnitState>();

    public UnitMoveAction(PlayerType player, Unit unit, Position from, Position to, UnitState previousState, UnitState newState)
        : base(player)
    {
        UnitType = unit.Type;
        FromPosition = from;
        ToPosition = to;
        PreviousState = previousState;
        NewState = newState;
        PreviousHealth = unit.Health.GetHealth();
        NewHealth = unit.Health.GetHealth(); // Will be updated after move if health changes
    }

    // Parameterless constructor for JSON deserialization
    public UnitMoveAction() : base() { }

    /// <summary>
    /// Update the new health value after the move is complete
    /// </summary>
    public void UpdateNewHealth(int newHealth)
    {
        NewHealth = newHealth;
    }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        return $"{playerName} moves {UnitType} from ({FromPosition.Row},{FromPosition.Column}) to ({ToPosition.Row},{ToPosition.Column})";
    }

    public override bool Execute(Board board)
    {
        // Find the unit at FromPosition
        var fromTile = board.Tiles.FirstOrDefault(t => t.Position.Row == FromPosition.Row && t.Position.Column == FromPosition.Column);
        var toTile = board.Tiles.FirstOrDefault(t => t.Position.Row == ToPosition.Row && t.Position.Column == ToPosition.Column);

        if (fromTile == null || toTile == null || fromTile.Unit == null)
            return false;

        Unit unit = fromTile.Unit;

        // Move the unit
        toTile.Unit = unit;
        toTile.Occupied = true;
        unit.Tile = toTile;
        unit.Position = toTile.Position;

        System.Diagnostics.Debug.WriteLine($"[UnitMoveAction.Execute] Setting {unit.Type} state from {unit.State} to {NewState}");
        unit.State = NewState;
        unit.Health.SetHealth(NewHealth);

        fromTile.Unit = null;
        fromTile.Occupied = false;

        // Lock the units that were recorded as locked by this move (during redo/load)
        if ((NewState == UnitState.Moved || NewState == UnitState.Marched) && LockedUnits.Count > 0)
        {
            foreach (var kvp in LockedUnits)
            {
                string key = kvp.Key;
                var parts = key.Split('-');
                PlayerType faction = Enum.Parse<PlayerType>(parts[0]);
                UnitType unitType = Enum.Parse<UnitType>(parts[1]);

                var lockedUnit = board.Units.FirstOrDefault(u => u.Faction == faction && u.Type == unitType);
                if (lockedUnit != null && lockedUnit != unit)
                {
                    lockedUnit.State = UnitState.Locked;
                    System.Diagnostics.Debug.WriteLine($"[UnitMoveAction.Execute] Locked {lockedUnit.Type} ({lockedUnit.Faction}) from {kvp.Value} (replay)");
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"[UnitMoveAction.Execute] Final state: {unit.State}");
        return true;
    }

    public override bool Undo(Board board)
    {
        // Find the unit at ToPosition (current position)
        var fromTile = board.Tiles.FirstOrDefault(t => t.Position.Row == FromPosition.Row && t.Position.Column == FromPosition.Column);
        var toTile = board.Tiles.FirstOrDefault(t => t.Position.Row == ToPosition.Row && t.Position.Column == ToPosition.Column);

        if (fromTile == null || toTile == null || toTile.Unit == null)
            return false;

        Unit unit = toTile.Unit;

        // Move the unit back
        fromTile.Unit = unit;
        fromTile.Occupied = true;
        unit.Tile = fromTile;
        unit.Position = fromTile.Position;
        unit.State = PreviousState;
        unit.Health.SetHealth(PreviousHealth);

        toTile.Unit = null;
        toTile.Occupied = false;

        // Unlock units that were locked by this move and restore their previous state
        foreach (var kvp in LockedUnits)
        {
            string key = kvp.Key;
            UnitState previousState = kvp.Value;
            var parts = key.Split('-');
            PlayerType faction = Enum.Parse<PlayerType>(parts[0]);
            UnitType unitType = Enum.Parse<UnitType>(parts[1]);

            var lockedUnit = board.Units.FirstOrDefault(u => u.Faction == faction && u.Type == unitType);
            if (lockedUnit != null && lockedUnit.State == UnitState.Locked)
            {
                lockedUnit.State = previousState;  // Restore to Idle or Ready
                System.Diagnostics.Debug.WriteLine($"[UnitMoveAction.Undo] Unlocked {lockedUnit.Type} ({lockedUnit.Faction}) to {previousState}");
            }
        }

        return true;
    }
}
