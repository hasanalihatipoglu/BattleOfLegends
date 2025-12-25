using System.Text.RegularExpressions;

namespace BoLLogic;

public class CavalryCounter(PlayerType faction) : Card
{

    public override CardType Type => CardType.CavalryCounter;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Counter;

    public override UnitClass Target => UnitClass.Cavalry;

    public override bool IsDiscard => true;


    public override bool IsValid()
    {
        // In Counter phase, the defender becomes the attacker
        Unit attacker = CombatManager.Instance.Attacker;
        Unit target = CombatManager.Instance.Target;

        if (attacker == null || target == null)
        {
            MessageController.Instance.Show("No Target!");
            return false;
        }


        if (CombatManager.Instance.CurrentCombatType != CombatType.Melee)
        {
            MessageController.Instance.Show("No Counter with Ranged!");
            return false;
        }


        // Check if the counterattacker (original defender) has the ability
        if (attacker.Abilities.Contains(Type) == false)
        {
            MessageController.Instance.Show("No Counter Ability!");
            return false;
        }

        return true;

    }


    public override bool Play()
    {

        if (TurnManager.Instance.MakeAttack(new CounterAttack(CombatManager.Instance.OriginalAttackPath))
            && IsValid())
        {
            return true;
        }

        else
        {
            CombatManager.Instance.ClearCombat(AttackType.Counter);
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