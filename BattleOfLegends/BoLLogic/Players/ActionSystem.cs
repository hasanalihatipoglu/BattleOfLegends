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
        int previousValue = ActionValue;

        if (ActionValue + actionAmount <= MaxAction)
        {
            ActionValue += actionAmount;
            IsMaxActionReached = false;
            System.Diagnostics.Debug.WriteLine($"Action changed for {Faction}: {actionAmount}");

            // Record action value change in history
            HistoryManager.Instance.RecordAction(
                new ActionValueChangeAction(Faction, previousValue, actionAmount)
            );
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
