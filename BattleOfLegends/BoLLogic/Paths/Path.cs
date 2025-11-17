


namespace BoLLogic;


public class Path
{
    public List<Tile> TilesInPath { get; set; } = [];

    internal Path Reverse()
    {
        return new Path
        {
            TilesInPath = TilesInPath.AsEnumerable().Reverse().ToList()
        };
    }
}
