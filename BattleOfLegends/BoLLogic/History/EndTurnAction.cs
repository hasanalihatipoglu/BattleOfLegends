namespace BoLLogic;

/// <summary>
/// Represents an END TURN action that increases action value and switches player
/// </summary>
public class EndTurnAction : GameAction
{
    public PlayerType PreviousPlayer { get; private set; }
    public PlayerType NewPlayer { get; private set; }
    public int PreviousActionValue { get; private set; }
    public int NewActionValue { get; private set; }

    public EndTurnAction(PlayerType previousPlayer, PlayerType newPlayer, int previousActionValue)
        : base(previousPlayer) // The player ending their turn
    {
        PreviousPlayer = previousPlayer;
        NewPlayer = newPlayer;
        PreviousActionValue = previousActionValue;
        NewActionValue = previousActionValue + 1;
    }

    public override string GetNotation()
    {
        string playerName = PreviousPlayer == PlayerType.Rome ? "Rome" : "Carthage";
        return $"{playerName} ends turn (Action: {PreviousActionValue} -> {NewActionValue})";
    }

    public override bool Execute(Board board)
    {
        // Increase action for the previous player
        var player = board.Players.FirstOrDefault(p => p.Type == PreviousPlayer);
        if (player != null)
        {
            player.Action.ActionValue = NewActionValue;
        }

        // Switch to new player
        TurnManager.Instance.CurrentPlayer = NewPlayer;
        TurnManager.Instance.CurrentTurn = NewPlayer; // Update whose turn it is
        TurnManager.Instance.TriggerPlayerChangeEvent();

        return true;
    }

    public override bool Undo(Board board)
    {
        // Switch back to previous player
        TurnManager.Instance.CurrentPlayer = PreviousPlayer;
        TurnManager.Instance.CurrentTurn = PreviousPlayer; // Restore whose turn it is
        TurnManager.Instance.TriggerPlayerChangeEvent();

        // Decrease action back to previous value
        var player = board.Players.FirstOrDefault(p => p.Type == PreviousPlayer);
        if (player != null)
        {
            player.Action.ActionValue = PreviousActionValue;
        }

        return true;
    }
}
