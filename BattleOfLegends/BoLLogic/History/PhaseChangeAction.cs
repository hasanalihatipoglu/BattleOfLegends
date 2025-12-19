namespace BoLLogic;

/// <summary>
/// Represents a turn phase change action that can be undone/redone
/// </summary>
public class PhaseChangeAction : GameAction
{
    public TurnPhase PreviousPhase { get; private set; }
    public TurnPhase NewPhase { get; private set; }

    public PhaseChangeAction(PlayerType player, TurnPhase previousPhase, TurnPhase newPhase)
        : base(player)
    {
        PreviousPhase = previousPhase;
        NewPhase = newPhase;
    }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        return $"{playerName} changes phase from {PreviousPhase} to {NewPhase}";
    }

    public override bool Execute(Board board)
    {
        TurnManager.Instance.CurrentTurnPhase = NewPhase;
        return true;
    }

    public override bool Undo(Board board)
    {
        TurnManager.Instance.CurrentTurnPhase = PreviousPhase;
        return true;
    }
}
