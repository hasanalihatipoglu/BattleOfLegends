namespace BoLLogic;

public class Leader(PlayerType faction) : Unit
{
    public override UnitType Type => UnitType.Leader;
    public override UnitClass Class => UnitClass.Cavalry;
    public override PlayerType Faction { get; } = faction;

    public override bool IsLight => true;
    public override int MarchMove => 3;
    public override int AttackMove => 2;
    public override int AttackRange => 1;
    public override int Strength => 3;

    public override int MeleeAttackPoint => 3;
    public override int MeleeDefensePoint => 3;
    public override int RangedAttackPoint => 3;
    public override int RangedDefensePoint => 3;


    private List<CardType> skills = new List<CardType>
    {
        CardType.Advance,
    };


    private List<CardType> abilities = new List<CardType>
    {
        //CardType.Withdraw,
        CardType.CavalryCharge,
        CardType.CavalryCounter,
        CardType.CavalryPursue,
        CardType.Flanking,
        CardType.Advance,
    };


    public override List<CardType> Skills
    {
        get => skills;
        set => skills = value;
    }

    public override List<CardType> Abilities
    {
        get => abilities;
        set => abilities = value;
    }


    public override string ToString()
    {
        return ($"{Faction}-{Type}");
    }

}
