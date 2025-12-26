

using System.Text.RegularExpressions;

namespace BoLLogic;

public class HitAndRun(PlayerType faction) : Card
{
    public override CardType Type => CardType.HitAndRun;

    public override PlayerType Faction { get; } = faction;

    public override TurnPhase Timing => TurnPhase.Advance;

    public override UnitClass Target => UnitClass.Cavalry;

    public override bool IsDiscard => true;


    public override bool IsValid()
    {
        if (CombatManager.Instance.OriginalAttackPath?.TilesInPath == null ||
            CombatManager.Instance.OriginalAttackPath.TilesInPath.Count == 0)
        {
            MessageController.Instance.Show("Invalid attack path!");
            return false;
        }

        Unit attacker = CombatManager.Instance.OriginalAttackPath.TilesInPath.First().Unit;


        if (attacker== null)
        {
            MessageController.Instance.Show("No Attacker!");
            return false;
        }

        TurnManager.Instance.SelectedUnit = attacker;

/*
        if (CombatManager.Instance.CurrentCombatType != CombatType.Ranged)
        {
            MessageController.Instance.Show("No Hit&Run with Melee!");
            return false;
        }
*/

        if (attacker.Abilities.Contains(Type) == false)
        {
            MessageController.Instance.Show("No Hit&Run Ability!");
            return false;
        }

        attacker.State = UnitState.Active;

        return true;

    }


    public override bool Play()
    {

        if (IsValid() == false)
            return false;

        if (CombatManager.Instance.OriginalAttackPath?.TilesInPath == null ||
            CombatManager.Instance.OriginalAttackPath.TilesInPath.Count == 0)
        {
            return false;
        }

        Unit attacker = CombatManager.Instance.OriginalAttackPath.TilesInPath.First().Unit;

        if (attacker == null)
            return false;

        PathFinder.Instance.FindPaths(attacker, attacker.Tile, PathType.HitAndRun);

        return true;

    }



    public override string ToString()
    {
        //return ($"{Type}");
        var regex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])");
        return regex.Replace($"{Type}", " ");

    }
}