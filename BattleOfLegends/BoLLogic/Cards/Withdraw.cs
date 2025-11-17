

using System.Text.RegularExpressions;

namespace BoLLogic;

public class Withdraw(PlayerType faction) : Card
{
    public override CardType Type => CardType.Withdraw;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Defend;

    public override UnitClass Target => UnitClass.None;

    public override bool IsDiscard => true;



    public override bool IsValid()
    {

        Unit target = CombatManager.Instance.Target;

        if (target == null)
        {
            MessageController.Instance.Show("No Target!");
            return false;
        }

       
        if (target.Abilities.Contains(Type) == false)
        {
            MessageController.Instance.Show("No Withdraw Ability!");
            return false;
        }

        return true;

    }


    public override bool Play()
    {

        if (IsValid() == false)
            return false;

        Unit target = CombatManager.Instance.Target;

        TurnManager.Instance.SelectedUnit = target;  

        PathFinder.Instance.FindPaths(target, target.Tile, PathType.Withdraw);

        if(PathFinder.Instance.CurrentSpaces.Count == 0)
        {
            MessageController.Instance.Show("No Tile to Withdraw!");
            return false;
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