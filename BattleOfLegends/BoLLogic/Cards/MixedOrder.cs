

using System.Text.RegularExpressions;

namespace BoLLogic;

public class MixedOrder(PlayerType faction) : Card
{
    public override CardType Type => CardType.MixedOrder;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Move;

    public override UnitClass Target => UnitClass.None;

    public override bool IsDiscard => true;


    public override bool IsValid()
    {

        return true;

    }


    public override bool Play()
    {

        if (IsValid() == false)
            return false;


        if(OrderManager.Instance.GiveOrder(Faction, OrderType.MixedOrder))
        {
            TurnManager.Instance.CurrentGamePhase = GamePhase.Order;
            TurnManager.Instance.ChangeCurrentGamePhase();

            MessageController.Instance.Show("Select 3 Units!");
            OrderManager.Instance.OrderLimit = 3;

        }

        return true;

    }


    public override string ToString()
    {
        //return ($"{Type}");
        var regex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])");
        return regex.Replace($"{Type}", " ");

    }
}