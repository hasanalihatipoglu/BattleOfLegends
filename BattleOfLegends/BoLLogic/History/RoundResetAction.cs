namespace BoLLogic;

/// <summary>
/// Represents the automatic state resets that happen at the start of certain rounds
/// </summary>
public class RoundResetAction : GameAction
{
    public int Round { get; set; }
    public bool ResetUnitStates { get; set; }
    public bool ResetActionValues { get; set; }

    public RoundResetAction(PlayerType player, int round, bool resetUnitStates, bool resetActionValues)
        : base(player)
    {
        Round = round;
        ResetUnitStates = resetUnitStates;
        ResetActionValues = resetActionValues;
    }

    // Parameterless constructor for JSON deserialization
    public RoundResetAction() : base() { }

    public override string GetNotation()
    {
        return $"Round {Round} begins - states reset";
    }

    public override bool Execute(Board board)
    {
        if (ResetUnitStates)
        {
            foreach (Unit unit in board.Units)
            {
                unit.State = UnitState.Idle;
            }
        }

        if (ResetActionValues)
        {
            foreach (Player player in board.Players)
            {
                player.Action.ActionValue = 0;
            }
        }

        return true;
    }

    public override bool Undo(Board board)
    {
        // Round resets cannot be undone - they represent the start of a new game phase
        // Unit states and action values should be restored by other actions
        return true;
    }
}
