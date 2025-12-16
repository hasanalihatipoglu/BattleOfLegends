namespace BoLLogic;

public class MoraleSystem()
{

    public int MoraleValue { get; set; }
    public PlayerType Faction { get; set; }


    public void Change(int moraleAmount)
    {
        MoraleValue += moraleAmount;
        System.Diagnostics.Debug.WriteLine($"Morale changed for {Faction}: {moraleAmount}");

        if (MoraleValue < 0) 
        {
            MoraleValue = 0;
        }

        if (MoraleValue == 0) 
        {
            End();
        }
    }

    public void End()
    {
        string winner = Faction == PlayerType.Rome ? "Carthage" : "Rome";
        MessageController.Instance.ShowWithOkButton($"GAME OVER ! {winner} wins!");
        TurnManager.Instance.CurrentGamePhase = GamePhase.End;
    }

}
