namespace BoLLogic;

public class CounterAttack(Path path) : Attack
{

    public override AttackType Type => AttackType.Counter;

    public override Path AttackPath { get; } = path;

    public override bool Execute()
    {
        
        if (AttackPath.TilesInPath.Count < 2)
        {
            return false;
        }
        

        if(CombatManager.Instance.DeclareCombat(Type, AttackPath.Reverse())==false )
        {
            return false;
        }


        return true;
        
    }

}