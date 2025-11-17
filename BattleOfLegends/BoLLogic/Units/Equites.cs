
namespace BoLLogic;

public class Equites(PlayerType faction) : Unit
{
    public override UnitType Type => UnitType.Equites;
    public override UnitClass Class => UnitClass.Cavalry;
    public override PlayerType Faction { get; } = faction;

    public override bool IsLight => false;
    public override int MarchMove => 3;
    public override int AttackMove => 2;
    public override int AttackRange => 1;
    public override int Strength => 2;
    public override int MeleeAttackPoint => 3;
    public override int MeleeDefensePoint => 3;
    public override int RangedAttackPoint => 3;
    public override int RangedDefensePoint => 3;


    private List<CardType> skills = new List<CardType> 
    {
       // CardType.CavalryCharge,
    };


    private List<CardType> abilities = new List<CardType>
    {
        CardType.Withdraw,
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
  