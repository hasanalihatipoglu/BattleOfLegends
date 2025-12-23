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
    public int HandValueBefore { get; private set; }
    public int HandValueAfter { get; private set; }

    public CardPlayAction(PlayerType player, Card card, CardState previousState, CardState newState, int handValueBefore, int handValueAfter)
        : base(player)
    {
        CardType = card.Type;
        PreviousState = previousState;
        NewState = newState;
        CardId = card.GetHashCode(); // Simple way to track specific card instance
        HandValueBefore = handValueBefore;
        HandValueAfter = handValueAfter;
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

        // Find the player
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
            return false;

        // Restore hand value to what it should be after this action
        player.Hand.HandValue = HandValueAfter;

        // Change card state
        card.State = NewState;

        // Notify UI that card state changed (e.g., clear _resolvingCard)
        card.NotifyStateChanged();

        // Trigger automatic state update to handle ReadyToPlay ↔ InHand transitions
        // This ensures the card is in the correct state based on current turn/phase
        card.On_Update(null, EventArgs.Empty);

        System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Execute] {Player} hand: {HandValueBefore} -> {HandValueAfter}, Card {CardType}: {PreviousState} -> {NewState}");
        return true;
    }

    public override bool Undo(Board board)
    {
        // Find the specific card
        var card = board.Cards.FirstOrDefault(c => c.Faction == Player && c.Type == CardType && c.GetHashCode() == CardId);
        if (card == null)
        {
            System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] FAILED: Card not found - {Player} {CardType}");
            return false;
        }

        // Find the player
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
        {
            System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] FAILED: Player not found - {Player}");
            return false;
        }

        System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] BEFORE: {Player} hand={player.Hand.HandValue}, Card {CardType} state={card.State}");

        // Restore hand value to what it was before this action
        player.Hand.HandValue = HandValueBefore;

        // Restore card state
        card.State = PreviousState;

        // Notify UI that card state changed (e.g., clear _resolvingCard)
        card.NotifyStateChanged();

        // Trigger automatic state update to handle ReadyToPlay ↔ InHand transitions
        // This ensures the card is in the correct state based on current turn/phase
        card.On_Update(null, EventArgs.Empty);

        System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] AFTER: {Player} hand={player.Hand.HandValue}, Card {CardType} state={card.State}");
        System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] Expected: hand {HandValueAfter} -> {HandValueBefore}, state {NewState} -> {PreviousState}");
        return true;
    }
}
