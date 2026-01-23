namespace BoLLogic;

/// <summary>
/// Represents a card play action that can be undone/redone
/// </summary>
public class CardPlayAction : GameAction
{
    public CardType CardType { get; set; }
    public CardState PreviousState { get; set; }
    public CardState NewState { get; set; }
    public int CardIndex { get; set; } // Index of this card among all cards of same type/faction
    public int HandValueBefore { get; set; }
    public int HandValueAfter { get; set; }

    public CardPlayAction(PlayerType player, Card card, CardState previousState, CardState newState, int handValueBefore, int handValueAfter, Board board)
        : base(player)
    {
        CardType = card.Type;
        PreviousState = previousState;
        NewState = newState;

        // Find the index of this card among all cards of the same type and faction
        var matchingCards = board.Cards.Where(c => c.Faction == player && c.Type == card.Type).ToList();
        CardIndex = matchingCards.IndexOf(card);

        HandValueBefore = handValueBefore;
        HandValueAfter = handValueAfter;
    }

    // Parameterless constructor for JSON deserialization
    public CardPlayAction() : base() { }

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
        // Find the specific card by index among matching cards
        var matchingCards = board.Cards.Where(c => c.Faction == Player && c.Type == CardType).ToList();
        if (CardIndex < 0 || CardIndex >= matchingCards.Count)
            return false;

        var card = matchingCards[CardIndex];

        // Find the player
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
            return false;

        // Change card state
        card.State = NewState;

        // Sync hand value from actual card count (more reliable than stored delta values)
        player.Hand.SyncFromCardCount(board);

        // Notify UI that card state changed (e.g., clear _resolvingCard)
        card.NotifyStateChanged();

        // Trigger automatic state update to handle ReadyToPlay ↔ InHand transitions
        // This ensures the card is in the correct state based on current turn/phase
        card.On_Update(null, EventArgs.Empty);

        System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Execute] {Player} Card {CardType}: {PreviousState} -> {NewState}, Hand synced to {player.Hand.HandValue}");
        return true;
    }

    public override bool Undo(Board board)
    {
        // Find the specific card by index among matching cards
        var matchingCards = board.Cards.Where(c => c.Faction == Player && c.Type == CardType).ToList();
        if (CardIndex < 0 || CardIndex >= matchingCards.Count)
        {
            System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] FAILED: Card not found - {Player} {CardType} index {CardIndex}");
            return false;
        }

        var card = matchingCards[CardIndex];

        // Find the player
        var player = board.Players.FirstOrDefault(p => p.Type == Player);
        if (player == null)
        {
            System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] FAILED: Player not found - {Player}");
            return false;
        }

        System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] BEFORE: {Player} hand={player.Hand.HandValue}, Card {CardType} state={card.State}");

        // Restore card state
        card.State = PreviousState;

        // Sync hand value from actual card count (more reliable than stored delta values)
        player.Hand.SyncFromCardCount(board);

        // Notify UI that card state changed (e.g., clear _resolvingCard)
        card.NotifyStateChanged();

        // Trigger automatic state update to handle ReadyToPlay ↔ InHand transitions
        // This ensures the card is in the correct state based on current turn/phase
        card.On_Update(null, EventArgs.Empty);

        System.Diagnostics.Debug.WriteLine($"[CardPlayAction.Undo] AFTER: {Player} hand={player.Hand.HandValue}, Card {CardType} state={card.State}");
        return true;
    }
}
