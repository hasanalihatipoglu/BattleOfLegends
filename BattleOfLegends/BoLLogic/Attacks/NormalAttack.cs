namespace BoLLogic;

public class NormalAttack(Path path) : Attack
{

    public override AttackType Type => AttackType.Normal;

    public override Path AttackPath { get; } = path;

    
    public override bool Execute()
    {

        if (AttackPath.TilesInPath.Count < 2)
        {
            return false;
        }


        if (CombatManager.Instance.DeclareCombat(Type, AttackPath) == false)
        {
            return false;
        }


        return true;

    }

}