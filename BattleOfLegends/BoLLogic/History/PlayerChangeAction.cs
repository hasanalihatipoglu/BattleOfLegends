namespace BoLLogic;

/// <summary>
/// Represents a player change action that can be undone/redone
/// </summary>
public class PlayerChangeAction : GameAction
{
    public PlayerType PreviousPlayer { get; private set; }
    public PlayerType NewPlayer { get; private set; }

    public PlayerChangeAction(PlayerType previousPlayer, PlayerType newPlayer)
        : base(newPlayer) // The new player is the one "taking the action"
    {
        PreviousPlayer = previousPlayer;
        NewPlayer = newPlayer;
    }

    public override string GetNotation()
    {
        string previousName = PreviousPlayer == PlayerType.Rome ? "Rome" : "Carthage";
        string newName = NewPlayer == PlayerType.Rome ? "Rome" : "Carthage";
        return $"Turn switches from {previousName} to {newName}";
    }

    public override bool Execute(Board board)
    {
        TurnManager.Instance.CurrentPlayer = NewPlayer;
        // Trigger the event to update UI
        TurnManager.Instance.TriggerPlayerChangeEvent();
        return true;
    }

    public override bool Undo(Board board)
    {
        TurnManager.Instance.CurrentPlayer = PreviousPlayer;
        // Trigger the event to update UI
        TurnManager.Instance.TriggerPlayerChangeEvent();
        return true;
    }
}
