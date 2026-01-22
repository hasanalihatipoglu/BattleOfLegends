namespace BoLLogic;

/// <summary>
/// Represents a game round change action that can be undone/redone
/// </summary>
public class GameRoundChangeAction : GameAction
{
    public int PreviousRound { get; set; }
    public int NewRound { get; set; }

    public GameRoundChangeAction(PlayerType player, int previousRound, int newRound)
        : base(player)
    {
        PreviousRound = previousRound;
        NewRound = newRound;
    }

    // Parameterless constructor for JSON deserialization
    public GameRoundChangeAction() : base() { }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        return $"{playerName} advances to round {NewRound} (from {PreviousRound})";
    }

    public override bool Execute(Board board)
    {
        TurnManager.Instance.CurrentGameRound = NewRound;
        board.GameRound = NewRound;
        return true;
    }

    public override bool Undo(Board board)
    {
        TurnManager.Instance.CurrentGameRound = PreviousRound;
        board.GameRound = PreviousRound;
        return true;
    }
}
