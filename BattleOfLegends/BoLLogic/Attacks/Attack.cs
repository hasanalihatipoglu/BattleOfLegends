
namespace BoLLogic;

public abstract class Attack
{
        public abstract AttackType Type { get; }

        public abstract Path AttackPath { get; }

        public abstract bool Execute();
}
