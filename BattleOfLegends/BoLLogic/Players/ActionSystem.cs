using System.ComponentModel;

namespace BoLLogic;

public class ActionSystem()
{
    public int MaxAction { get; set; }
    public int ActionValue { get; set; }
    public PlayerType Faction { get; set; }
    public bool IsMaxActionReached { get; set; }

    public void Change(int actionAmount)
    {
        if (ActionValue + actionAmount <= MaxAction)
        {
            ActionValue += actionAmount;
            IsMaxActionReached = false;
        }

        else 
        {
            End();
            IsMaxActionReached = true;
        }
       
    }

    public void End()
    {
        MessageController.Instance.Show("MAX ACTION REACHED !");
    }
}
