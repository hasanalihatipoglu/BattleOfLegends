namespace BoLLogic;

public class Archer(PlayerType faction) : Unit
{
    public override UnitType Type => UnitType.Archer;
    public override UnitClass Class => UnitClass.Infantry;
    public override PlayerType Faction { get; } = faction;

    public override bool IsLight => true;
    public override int MarchMove => 2;
    public override int AttackMove => 0;
    public override int AttackRange => 3;
    public override int Strength => 2;

    public override int MeleeAttackPoint => 4;
    public override int MeleeDefensePoint => 4;
    public override int RangedAttackPoint => 5;
    public override int RangedDefensePoint => 4;


    private List<CardType> skills = new List<CardType>
    {
        // CardType.CavalryCharge,
    };


    private List<CardType> abilities = new List<CardType>
    {
        CardType.Withdraw,
        CardType.Flanking
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
