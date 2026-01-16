namespace BoLLogic;

/// <summary>
/// Represents a turn phase change action that can be undone/redone
/// </summary>
public class PhaseChangeAction : GameAction
{
    public TurnPhase PreviousPhase { get; set; }
    public TurnPhase NewPhase { get; set; }

    // Optional: Include attacker and target information when changing to Attack phase
    public UnitType? AttackerType { get; set; }
    public UnitType? TargetType { get; set; }

    public PhaseChangeAction(PlayerType player, TurnPhase previousPhase, TurnPhase newPhase,
        UnitType? attackerType = null, UnitType? targetType = null)
        : base(player)
    {
        PreviousPhase = previousPhase;
        NewPhase = newPhase;
        AttackerType = attackerType;
        TargetType = targetType;
    }

    // Parameterless constructor for JSON deserialization
    public PhaseChangeAction() : base() { }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";

        // Include attacker and target information when changing to Attack phase
        if (NewPhase == TurnPhase.Attack && AttackerType.HasValue && TargetType.HasValue)
        {
            return $"{playerName} changes phase from {PreviousPhase} to {NewPhase} ({AttackerType.Value} attacks {TargetType.Value})";
        }

        return $"{playerName} changes phase from {PreviousPhase} to {NewPhase}";
    }

    public override bool Execute(Board board)
    {
        TurnManager.Instance.CurrentTurnPhase = NewPhase;

        // Some phases require switching the current player
        if (NewPhase == TurnPhase.Move || NewPhase == TurnPhase.Defend ||
            NewPhase == TurnPhase.Roll || NewPhase == TurnPhase.Counter ||
            NewPhase == TurnPhase.Advance)
        {
            TurnManager.Instance.CurrentPlayer = TurnManager.Instance.CurrentPlayer.Opponent();
        }

        return true;
    }

    public override bool Undo(Board board)
    {
        TurnManager.Instance.CurrentTurnPhase = PreviousPhase;

        // When undoing, reverse the player switch if needed
        if (NewPhase == TurnPhase.Move || NewPhase == TurnPhase.Defend ||
            NewPhase == TurnPhase.Roll || NewPhase == TurnPhase.Counter ||
            NewPhase == TurnPhase.Advance)
        {
            TurnManager.Instance.CurrentPlayer = TurnManager.Instance.CurrentPlayer.Opponent();
        }

        return true;
    }
}
