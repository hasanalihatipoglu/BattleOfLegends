namespace BoLLogic;

public class Water: Tile
{
    public override TileType Type => TileType.Water;

    public override bool Passable => false;

    public override bool AttackPassable => true;

    public override string ToString()
    {
        return ($"{Type}");
    }
}
