namespace BoLLogic;

public abstract class Card : IDisposable
{
    private bool _disposed = false;

    public abstract CardType Type { get; }
    public abstract PlayerType Faction { get; }
    public abstract TurnPhase Timing { get; }
    public abstract UnitClass Target { get; }
    public abstract bool IsDiscard { get; }

    public CardPosition Position { get; set; }
    public Unit Unit { get; set; }
    public CardState State { get; set; }
 

   // CardPosition playPosition = new CardPosition(20, 300);
   

    public event EventHandler ChangeState;



    public virtual bool IsValid()
    {
        return true;
    }


    public virtual bool Play()
    {
        return true;
    }


    public virtual bool Discard()
    {
        return true;
    }



    public void On_Update(object sender, EventArgs e)
    {
        // Only process cards that are in deck or hand (not already played/discarded)
        if (this.State != CardState.InDeck &&
            this.State != CardState.InHand &&
            this.State != CardState.ReadyToPlay)
        {
            return;
        }

        // Check if card should be ready to play (based on player turn and phase)
        bool shouldBeReady = (this.Faction == TurnManager.Instance.CurrentPlayer
                           && this.Timing == TurnManager.Instance.CurrentTurnPhase
                           && (this.State == CardState.InHand || this.State == CardState.ReadyToPlay));

        if (shouldBeReady && this.State == CardState.InHand)
        {
            // Transition from InHand to ReadyToPlay
            ChangeCardState(CardState.ReadyToPlay);
        }
        else if (!shouldBeReady && this.State == CardState.ReadyToPlay)
        {
            // Card is no longer ready - change back to InHand
            ChangeCardState(CardState.InHand);
        }
    }



    public void OnClick()
    {
        SoundController.Instance.PlaySound("flip_card");

        TurnManager.Instance.SelectedCard = this;

        if (TurnManager.Instance.CurrentGamePhase == GamePhase.Select)
        {
            switch (this.State)
            {
                case CardState.InDeck:
                    // Check if adding one more card would exceed max hand limit
                    var player = GameManager.Instance.CurrentBoard.Players.FirstOrDefault(p => p.Type == Faction);
                    if (player != null && player.Hand.HandValue >= player.Hand.MaxHand)
                    {
                        MessageController.Instance.Show($"Max hand limit reached for {Faction}!");
                        return;
                    }

                    TurnManager.Instance.ChangeCurrentPlayerHand(Faction, 1);
                    ChangeCardState(CardState.InHand);
                    break;

                case CardState.InHand:
                    TurnManager.Instance.ChangeCurrentPlayerHand(this.Faction, -1);
                    ChangeCardState(CardState.InDeck);
                    break;
            }
        }

        else
        { 
            if (TurnManager.Instance.CurrentPlayer != this.Faction)
                return;

            switch (this.State)
            {
                case CardState.ReadyToPlay:
                    if(TurnManager.Instance.PlayCard(this)==true)
                    {
                        // Card moves to center for resolution
                        ChangeCardState(CardState.Resolving);
                    }
                    break;

                case CardState.Resolving:
                    // Click on resolving card to discard it
                    if (IsDiscard)
                    {
                        TurnManager.Instance.ChangeCurrentPlayerHand(this.Faction, -1);
                        ChangeCardState(CardState.Discarded);
                    }
                    else
                    {
                        // Non-discard cards go back to hand
                        ChangeCardState(CardState.InHand);
                    }
                    break;
            }

        }
          
    }


    public void ChangeCardState(CardState state)
    {
        var oldState = this.State;
        this.State = state;
        System.Diagnostics.Debug.WriteLine($"*** CARD STATE CHANGED: {this.Type} ({this.Faction}) from {oldState} -> {state}");

        // Record card state change in history (skip ReadyToPlay transitions as they're automatic)
        if (oldState != CardState.ReadyToPlay && state != CardState.ReadyToPlay)
        {
            HistoryManager.Instance.RecordAction(
                new CardPlayAction(this.Faction, this, oldState, state)
            );
        }

        ChangeState?.Invoke(this, EventArgs.Empty);
    }

    // IDisposable implementation to clean up event subscriptions
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Unsubscribe from events to prevent memory leaks
            TurnManager.Instance.ChangeTurnPhase -= On_Update;
        }

        _disposed = true;
    }

}
