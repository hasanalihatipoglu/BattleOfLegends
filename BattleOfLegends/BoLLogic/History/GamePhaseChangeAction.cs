namespace BoLLogic;

/// <summary>
/// Represents a game phase change action that can be undone/redone
/// </summary>
public class GamePhaseChangeAction : GameAction
{
    public GamePhase PreviousPhase { get; set; }
    public GamePhase NewPhase { get; set; }

    public GamePhaseChangeAction(PlayerType player, GamePhase previousPhase, GamePhase newPhase)
        : base(player)
    {
        PreviousPhase = previousPhase;
        NewPhase = newPhase;
    }

    // Parameterless constructor for JSON deserialization
    public GamePhaseChangeAction() : base() { }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        return $"{playerName} changes game phase from {PreviousPhase} to {NewPhase}";
    }

    public override bool Execute(Board board)
    {
        TurnManager.Instance.CurrentGamePhase = NewPhase;
        return true;
    }

    public override bool Undo(Board board)
    {
        TurnManager.Instance.CurrentGamePhase = PreviousPhase;
        return true;
    }
}
