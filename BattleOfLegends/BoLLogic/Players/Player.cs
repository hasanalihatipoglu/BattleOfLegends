namespace BoLLogic;

public enum PlayerType
{
    None,
    Rome,
    Carthage
}

public class Player(PlayerType type)
{
    public PlayerType Type { get; } = type;
    public MoraleSystem Morale { get; set; }
    public HandSystem Hand { get; set; }
    public ActionSystem Action { get; set; }
    public Unit Leader { get; set; }


    public void On_MoraleChanged(object sender, MoraleEventArgs e)
    {
        if (Type == e.Faction)
        {
            Morale.Change(e.MoraleAmount);


        }
    }

    public void On_HandChanged(object sender, HandEventArgs e)
    {
        if (Type == e.Faction) 
        {
            Hand.Change(e.HandAmount);
        }
    }

    public void On_ActionChanged(object sender, ActionEventArgs e)
    {
        if (Type == e.Faction)
        {
            Action.Change(e.ActionAmount);

        }
    }

    public void On_Update(object sender, EventArgs e)
    {

        if (sender is Card)
        {
            Card card = (Card)sender;

            if (Type == card.Faction && TurnManager.Instance.CurrentGamePhase == GamePhase.Select)
            {
                if (card.State == CardState.InHand)
                {
                    if (Hand.IsMaxHandReached)
                    {

                        card.ChangeCardState(CardState.InDeck);
                    }
                    

                    else
                    {

                    }

                    
                                       
                }

              
            }
        }


    }

}

 

public static class PlayerTypeExtensions
{
    public static PlayerType Opponent(this PlayerType PlayerType)
    {
        return PlayerType switch
        {
            PlayerType.Rome => PlayerType.Carthage,
            PlayerType.Carthage => PlayerType.Rome,
            _ => PlayerType.None,
        };
    }
}

