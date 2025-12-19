namespace BoLLogic;

/// <summary>
/// Represents a card play action that can be undone/redone
/// </summary>
public class CardPlayAction : GameAction
{
    public CardType CardType { get; private set; }
    public CardState PreviousState { get; private set; }
    public CardState NewState { get; private set; }
    public int CardId { get; private set; } // To uniquely identify which card if player has multiple of same type

    public CardPlayAction(PlayerType player, Card card, CardState previousState, CardState newState)
        : base(player)
    {
        CardType = card.Type;
        PreviousState = previousState;
        NewState = newState;
        CardId = card.GetHashCode(); // Simple way to track specific card instance
    }

    public override string GetNotation()
    {
        string playerName = Player == PlayerType.Rome ? "Rome" : "Carthage";
        string stateDescription = NewState switch
        {
            CardState.Resolving => "plays",
            CardState.InDeck => "returns to deck",
            CardState.Discarded => "discards",
            _ => "changes state of"
        };
        return $"{playerName} {stateDescription} {CardType} card";
    }

    public override bool Execute(Board board)
    {
        // Find the specific card
        var card = board.Cards.FirstOrDefault(c => c.Faction == Player && c.Type == CardType && c.GetHashCode() == CardId);
        if (card == null)
            return false;

        card.State = NewState;
        return true;
    }

    public override bool Undo(Board board)
    {
        // Find the specific card
        var card = board.Cards.FirstOrDefault(c => c.Faction == Player && c.Type == CardType && c.GetHashCode() == CardId);
        if (card == null)
            return false;

        card.State = PreviousState;
        return true;
    }
}
