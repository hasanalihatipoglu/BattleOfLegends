namespace BoLLogic;

public class Velites(PlayerType faction) : Unit
{
    public override UnitType Type => UnitType.Velites;
    public override UnitClass Class => UnitClass.Infantry;
    public override PlayerType Faction { get; } = faction;

    public override bool IsLight => true;
    public override int MarchMove => 3;
    public override int AttackMove => 1;
    public override int AttackRange => 2;
    public override int Strength => 2;

    public override int MeleeAttackPoint => 5;
    public override int MeleeDefensePoint => 5;
    public override int RangedAttackPoint => 5;
    public override int RangedDefensePoint => 5;


    private List<CardType> skills = new List<CardType>
    {
        // CardType.CavalryCharge,
    };


    private List<CardType> abilities = new List<CardType>
    {
        CardType.Withdraw,
        CardType.Skirmish,
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
