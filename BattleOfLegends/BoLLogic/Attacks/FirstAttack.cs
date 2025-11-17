namespace BoLLogic;

public class FirstAttack(Path path) : Attack
{

    public override AttackType Type => AttackType.First;

    public override Path AttackPath { get; } = path;


    public override bool Execute()
    {
    
        if (AttackPath.TilesInPath.Count < 2)
        {
            return false;
        }


        if (CombatManager.Instance.DeclareCombat(Type, AttackPath.Reverse()) == false)
        {
            return false;
        }


        return true;
 
    }

}