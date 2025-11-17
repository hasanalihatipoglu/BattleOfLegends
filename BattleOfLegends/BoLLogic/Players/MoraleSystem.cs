namespace BoLLogic;

public class MoraleSystem()
{

    public int MoraleValue { get; set; }
    public PlayerType Faction { get; set; }


    public void Change(int moraleAmount)
    {
        MoraleValue += moraleAmount;

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
        MessageController.Instance.Show("GAME OVER !");
    }

}
