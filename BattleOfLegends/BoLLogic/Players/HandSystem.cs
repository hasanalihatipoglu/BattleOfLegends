using System.ComponentModel;

namespace BoLLogic;

public class HandSystem()
{
    public int MaxHand { get; set; }
    public int HandValue { get; set; }
    public PlayerType Faction { get; set; }
    public bool IsMaxHandReached { get; set; }

    public void Change(int handAmount)
    {
        if (HandValue + handAmount <= MaxHand)
        {
            HandValue += handAmount;
            IsMaxHandReached = false;
            System.Diagnostics.Debug.WriteLine($"Hand changed for {Faction}: {handAmount}");
        }

        else 
        {
            End();
            IsMaxHandReached = true;
        }
       
    }

    public void End()
    {
        MessageController.Instance.Show("MAX HAND REACHED !");
    }
}
