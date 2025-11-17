using System.Text.RegularExpressions;

namespace BoLLogic;

public class FirstStrike(PlayerType faction) : Card
{
    public override CardType Type => CardType.FirstStrike;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Defend;

    public override UnitClass Target => UnitClass.Infantry;

    public override bool IsDiscard => true;


    public override bool IsValid()
    {

        Unit target = CombatManager.Instance.Target;

        if (target == null)
        {
            MessageController.Instance.Show("No Target!");
            return false;
        }


        if (CombatManager.Instance.CurrentCombatType != CombatType.Melee)
        {
            MessageController.Instance.Show("No FirstStrike with Ranged!");
            return false;
        }


        if (target.Abilities.Contains(Type) == false)
        {
            MessageController.Instance.Show("No FirstStrike Ability!");
            return false;
        }

        return true;

    }


    public override bool Play()
    {

        TurnManager.Instance.MakeAttack(new FirstAttack(CombatManager.Instance.OriginalAttackPath));

        if ( IsValid() )
        { 
            return true;        
        }
        else
        {
            CombatManager.Instance.ClearCombat(AttackType.First);
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