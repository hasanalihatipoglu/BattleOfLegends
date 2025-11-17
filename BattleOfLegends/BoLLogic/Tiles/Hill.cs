namespace BoLLogic;

public class Hill : Tile
{
    public override TileType Type => TileType.Water;

    public override bool Passable => false;

    public override bool AttackPassable => false;

    public override string ToString()
    {
        return ($"{Type}");
    }
}
