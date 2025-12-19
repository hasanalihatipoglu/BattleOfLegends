namespace BoLLogic;

/// <summary>
/// Represents a unit movement action that can be undone/redone
/// </summary>
public class UnitMoveAction : GameAction
{
    public UnitType UnitType { get; private set; }
    public Position FromPosition { get; private set; }
    public Position ToPosition { get; private set; }
    public UnitState PreviousState { get; private set; }
    public UnitState NewState { get; private set; }
    public int PreviousHealth { get; private set; }
    public int NewHealth { get; private set; }

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
        unit.State = NewState;
        unit.Health.SetHealth(NewHealth);

        fromTile.Unit = null;
        fromTile.Occupied = false;

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

        return true;
    }
}
