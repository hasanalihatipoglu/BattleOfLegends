using System;

namespace BoLLogic;


public enum OrderType
{
    MixedOrder,
    InfantryAssault,
    CavalryAssault,
    GroupOrder,
    LineOrder,
    Outflank,
    Formation,
}


public sealed class OrderManager
{

    private static readonly Lazy<OrderManager> instance = new Lazy<OrderManager>(() => new OrderManager());

    public static OrderManager Instance => instance.Value;

    public OrderType Type { get; set; }
    public PlayerType Faction { get; set; }
    public Player ActivePlayer { get; set; }

    public bool IsOrderGiven { get; set; }

    public int OrderLimit{ get; set; }
    public int NumberOfOrderedUnits { get; set; }


    public event EventHandler<StateChangedEventArgs> ChangeUnitState;


    public bool GiveOrder(PlayerType faction, OrderType type)
    {

        Type = type;
        Faction = faction;

        foreach (Player player in GameManager.Instance.CurrentBoard.Players)
        {
            if (Faction == player.Type)
            {
                ActivePlayer = player;
            }
        }
        

        if (CheckOrder(type) == false)
            return false;


        IsOrderGiven = true;

        return true;

    }



    public bool CheckOrder(OrderType type)
    {
        return true;
    }


    public void EndOrder()
    {
        if(IsOrderGiven)
        {
            ChangeUnitState?.Invoke(this, new StateChangedEventArgs(ActivePlayer.Leader, UnitState.Passive));  
        }
  
    }


    public void Reset()
    {
        NumberOfOrderedUnits = 0;
        IsOrderGiven = false;
    }

    /// <summary>
    /// Counts the actual number of units in Ready state for the given faction.
    /// This is more reliable than tracking deltas, especially during undo/redo.
    /// </summary>
    public int CountReadyUnits(PlayerType faction, Board board)
    {
        return board.Units.Count(u => u.Faction == faction && u.State == UnitState.Ready);
    }


}





