namespace BoLLogic;

/// <summary>
/// Represents a change in a player's action value
/// </summary>
public class ActionValueChangeAction : GameAction
{
    public int PreviousValue { get; set; }
    public int NewValue { get; set; }
    public int Delta { get; set; }

    public ActionValueChangeAction(PlayerType player, int previousValue, int delta)
        : base(player)
    {
        PreviousValue = previousValue;
        Delta = delta;
        NewValue = previousValue + delta;
    }

    // Parameterless constructor for JSON deserialization
    public ActionValueChangeAction() : base() { }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        string change = Delta > 0 ? $"+{Delta}" : Delta.ToString();
        return $"{playerName} action changes {change} (from {PreviousValue} to {NewValue})";
    }

    public override bool Execute(Board board)
    {
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
            return false;

        player.Action.ActionValue = NewValue;
        return true;
    }

    public override bool Undo(Board board)
    {
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
            return false;

        player.Action.ActionValue = PreviousValue;
        return true;
    }
}
