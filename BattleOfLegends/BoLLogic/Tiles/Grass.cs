namespace BoLLogic;

public class Grass : Tile
{
    public override TileType Type => TileType.Grass;

    public override bool Passable => true;

    public override bool AttackPassable => true;

    public override string ToString()
    {
        return ($"{Type}");
    }
}