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

        MovePath.TilesInPath.Last().Unit = unit;
        MovePath.TilesInPath.Last().Occupied = true;
        unit.Tile = MovePath.TilesInPath.Last();
        unit.Position = unit.Tile.Position;

        MovePath.TilesInPath.First().Unit = null;
        MovePath.TilesInPath.First().Occupied = false;

        // Record in history after executing
        UnitState newState = unit.State;
        HistoryManager.Instance.RecordAction(
            new UnitMoveAction(unit.Faction, unit, fromPosition, toPosition, previousState, newState)
        );

        return true;
    }               
}




