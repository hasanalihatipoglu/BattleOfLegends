
namespace BoLLogic;

public sealed class RegionBuilder
{

    private static RegionBuilder instance = null;

    public static RegionBuilder Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RegionBuilder();
            }
            return instance;
        }
    }


    public Region CurrentRegion { get; set; } = new();
    public Board CurrentBoard { get; set; } = new();

    public Tile OriginalSpace { get; set; } = null;
    public Tile Highlight { get; set; } = null;

    public event EventHandler Update;


    public void BuildRegion( Tile origin, RegionType type)
    {

        Reset(type);

        Finalize(origin, type);
    }


 

    void AddToRegion(Tile tile, RegionType type)
    {
        switch (type)
        {
            case RegionType.Line:
                break;
        }
    }


    void Finalize(Tile origin, RegionType type)
    {

        switch (type)
        {
            case RegionType.Line:              
                break;

        }

    }

      


    public void Reset(RegionType type)
    {

        switch (type)
        {
            case RegionType.Line:
           
                break;
        }

    }


}

