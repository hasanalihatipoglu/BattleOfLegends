namespace BoLLogic;

public class NormalMove(Path path) : Move
{
    public override MoveType Type => MoveType.Normal;

    public override Path MovePath { get; } = path;

    public override bool Execute()
    {

        if (MovePath.TilesInPath.Count < 2)
        {
            return false;
        }

        Unit unit = MovePath.TilesInPath.First().Unit;

        if (unit == null)
        {
            return false;
        }

        // Record the move in history before executing
        Position fromPosition = MovePath.TilesInPath.First().Position;
        Position toPosition = MovePath.TilesInPath.Last().Position;
        UnitState previousState = unit.State;
        int previousHealth = unit.Health.GetHealth();

        MovePath.TilesInPath.Last().Unit = unit;
        MovePath.TilesInPath.Last().Occupied = true;
        unit.Tile = MovePath.TilesInPath.Last();
        unit.Position = unit.Tile.Position;

        MovePath.TilesInPath.First().Unit = null;
        MovePath.TilesInPath.First().Occupied = false;

        // Determine and set the new state based on move distance
        UnitState newState;
        if (MovePath.TilesInPath.Count - 1 > unit.AttackMove)
        {
            newState = UnitState.Marched;
        }
        else
        {
            newState = UnitState.Moved;
        }
        unit.State = newState;

        // Record in history after executing with the correct final state
        int newHealth = unit.Health.GetHealth();
        var moveAction = new UnitMoveAction(unit.Faction, unit, fromPosition, toPosition, previousState, newState);
        moveAction.UpdateNewHealth(newHealth);

        // Lock all other friendly Idle/Ready units if this unit moved or marched
        var board = GameManager.Instance.CurrentBoard;
        if (newState == UnitState.Moved || newState == UnitState.Marched)
        {
            foreach (Unit u in board.Units)
            {
                if (u.Faction == unit.Faction && u != unit && (u.State == UnitState.Idle || u.State == UnitState.Ready))
                {
                    string key = $"{u.Faction}-{u.Type}";
                    UnitState previousUnitState = u.State;  // Capture state before locking
                    u.StateBeforeLocked = previousUnitState;  // Store for EndTurnAction to restore
                    u.State = UnitState.Locked;
                    moveAction.LockedUnits[key] = previousUnitState;  // Store previous state for undo
                    System.Diagnostics.Debug.WriteLine($"[NormalMove.Execute] Locked {u.Type} ({u.Faction}) from {previousUnitState}");
                }
            }
        }

        HistoryManager.Instance.RecordAction(moveAction);

        return true;
    }               
}




