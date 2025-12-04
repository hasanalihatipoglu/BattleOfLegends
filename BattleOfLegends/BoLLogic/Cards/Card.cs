namespace BoLLogic;

public abstract class Card
{

    public abstract CardType Type { get; }
    public abstract PlayerType Faction { get; }
    public abstract TurnPhase Timing { get; }
    public abstract UnitClass Target { get; }
    public abstract bool IsDiscard { get; }

    public CardPosition Position { get; set; }
    public Unit Unit { get; set; }
    public CardState State { get; set; }
 

    CardPosition playPosition = new CardPosition(20, 300);
   

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
        System.Diagnostics.Debug.WriteLine($"On_Update: {this.Type} ({this.Faction}) - State: {this.State}, CurrentPlayer: {TurnManager.Instance.CurrentPlayer}, CurrentPhase: {TurnManager.Instance.CurrentTurnPhase}, Timing: {this.Timing}");

        if (this.Faction == TurnManager.Instance.CurrentPlayer
            && this.Timing == TurnManager.Instance.CurrentTurnPhase
            && (this.State == CardState.InHand || this.State == CardState.ReadyToPlay))

        {
            System.Diagnostics.Debug.WriteLine($"  -> Setting {this.Type} to ReadyToPlay");
            //this.State = CardState.ReadyToPlay;
              ChangeCardState(CardState.ReadyToPlay);  //----> Triggers Form1's On_Update causing Blinking
        }


        else if (this.State == CardState.ReadyToPlay)
        {
            System.Diagnostics.Debug.WriteLine($"  -> Setting {this.Type} back to InHand");
            //this.State = CardState.InHand;
              ChangeCardState(CardState.InHand);  //----> Triggers Form1's On_Update causing Blinking
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
                    TurnManager.Instance.ChangeCurrentPlayerHand(this.Faction, 1);                    
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
                    if(TurnManager.Instance.PlayCard(this)==true
                        && IsDiscard==true)
                    {
                        TurnManager.Instance.ChangeCurrentPlayerHand(this.Faction, -1);
                        ChangeCardState(CardState.Played);
                        this.Position = playPosition; 
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
        ChangeState?.Invoke(this, EventArgs.Empty);
    }

}
