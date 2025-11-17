

using System.Text.RegularExpressions;

namespace BoLLogic;

public class Flanking(PlayerType faction) : Card
{
    public override CardType Type => CardType.Flanking;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Roll;

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


        if (target.Tile == null)
        {
            MessageController.Instance.Show("No Target Tile!");
            return false;
        }


        if (CombatManager.Instance.CurrentCombatType != CombatType.Melee)
        {
            MessageController.Instance.Show("No Flanking with Ranged!");
            return false;
        }
            

        foreach (Tile tile in target.Tile.Adjacents)
        {
            if (tile != null && tile.Unit != null)
            {
                if (tile.Unit.Faction == Faction && tile.Unit != CombatManager.Instance.Attacker)
                {
                    if(tile.Unit.Abilities.Contains(Type) == true)
                    return true;
                }
            }
        }

        return false;
    }


    public override bool Play()
    {
        if (IsValid())
        {
            CombatManager.Instance.DiceModifier++;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override string ToString()
    {
        //return ($"{Type}");
        var regex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])");
        return regex.Replace($"{Type}", " ");

    }
}