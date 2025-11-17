using System.Text.RegularExpressions;

namespace BoLLogic;

public class CavalryCharge(PlayerType faction) : Card
{
    public override CardType Type => CardType.CavalryCharge;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Attack;

    public override UnitClass Target => UnitClass.Cavalry;
    
    public override bool IsDiscard => true;


    public override bool IsValid()
    {

        if (PathFinder.Instance.OriginalSpace!=null
            && CombatManager.Instance.Target != null
            && PathFinder.Instance.OriginalSpace.Adjacents.Contains(CombatManager.Instance.Target.Tile))
        {
            MessageController.Instance.Show("No Charge Distance!");
            return false;
        }
           

        if (CombatManager.Instance.CurrentCombatType != CombatType.Melee)
        {
            MessageController.Instance.Show("No Charge with Ranged!");
            return false;
        }


        if (CombatManager.Instance.Attacker!=null 
            && CombatManager.Instance.Attacker.Abilities.Contains(Type) == false)
        {
            MessageController.Instance.Show("No Charge Ability!");
            return false;
        }


        return true;

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