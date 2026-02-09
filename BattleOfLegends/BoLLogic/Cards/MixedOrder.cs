

using System.Text.RegularExpressions;

namespace BoLLogic;

public class MixedOrder(PlayerType faction) : Card
{
    public override CardType Type => CardType.MixedOrder;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Move;

    public override UnitClass Target => UnitClass.None;

    public override CardClass Class => CardClass.Order;

    public override bool IsDiscard => true;


    public override bool IsValid()
    {
        // Find the leader for this faction
        var board = GameManager.Instance.CurrentBoard;
        var player = board.Players.FirstOrDefault(p => p.Type == Faction);

        if (player?.Leader == null)
        {
            MessageController.Instance.Show("No Leader!");
            return false;
        }

        // Check if leader has already acted (moved, marched, or attacked) - cannot play order card
        var leaderState = player.Leader.State;
        if (leaderState == UnitState.Moved ||
            leaderState == UnitState.Marched ||
            leaderState == UnitState.Attacked ||
            leaderState == UnitState.Passive ||
            leaderState == UnitState.Advanced ||
            leaderState == UnitState.PushedBack)
        {
            MessageController.Instance.Show("Leader has already acted - cannot give orders!");
            return false;
        }

        return true;

    }


    public override bool Play()
    {

        if (IsValid() == false)
            return false;


        if(OrderManager.Instance.GiveOrder(Faction, OrderType.MixedOrder))
        {
            // Set the leader's state to Ordered (leader has given an order)
            var board = GameManager.Instance.CurrentBoard;
            var player = board.Players.FirstOrDefault(p => p.Type == Faction);
            if (player?.Leader != null)
            {
                player.Leader.State = UnitState.Ordered;
            }

            GamePhase previousPhase = TurnManager.Instance.CurrentGamePhase;
            TurnManager.Instance.CurrentGamePhase = GamePhase.Order;
            TurnManager.Instance.ChangeCurrentGamePhase();

            // Record game phase change in history
            HistoryManager.Instance.RecordAction(
                new GamePhaseChangeAction(Faction, previousPhase, GamePhase.Order)
            );

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