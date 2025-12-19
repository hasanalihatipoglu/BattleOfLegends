namespace BoLLogic;

/// <summary>
/// Represents a change in a player's morale value
/// </summary>
public class MoraleChangeAction : GameAction
{
    public int PreviousValue { get; private set; }
    public int NewValue { get; private set; }
    public int Delta { get; private set; }

    public MoraleChangeAction(PlayerType player, int previousValue, int delta)
        : base(player)
    {
        PreviousValue = previousValue;
        Delta = delta;
        NewValue = previousValue + delta;
    }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        string change = Delta > 0 ? $"+{Delta}" : Delta.ToString();
        return $"{playerName} morale changes {change} (from {PreviousValue} to {NewValue})";
    }

    public override bool Execute(Board board)
    {
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
            return false;

        player.Morale.MoraleValue = NewValue;
        return true;
    }

    public override bool Undo(Board board)
    {
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
            return false;

        player.Morale.MoraleValue = PreviousValue;
        return true;
    }
}
