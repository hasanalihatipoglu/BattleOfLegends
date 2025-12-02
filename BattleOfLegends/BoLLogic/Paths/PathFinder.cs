
namespace BoLLogic;

public sealed class PathFinder
{

    private static readonly Lazy<PathFinder> instance = new Lazy<PathFinder>(() => new PathFinder());

    public static PathFinder Instance => instance.Value;


    public Frontier CurrentFrontier { get; set; } = new();
    public Frontier CurrentAttackFrontier { get; set; } = new();

    public Path CurrentPath { get; set; } = new();
    public Path CurrentAttackPath { get; set; } = new();

    public Dictionary<Tile, Path> CurrentSpaces { get; set; } = new();
    public Dictionary<Tile, (Path MovePath, Path AttackPath)> CurrentTargets { get; set; } = new();

    public List<Tile> CurrentEngagements { get; set; } = new();

    public Board CurrentBoard { get; set; } = new();

    public Tile OriginalSpace { get; set; } = null;
    public Tile Highlight { get; set; } = null;

    public event EventHandler Update;


    public void FindPaths(Unit unit, Tile origin, PathType type)
    {

        Reset(type);

        Queue<Tile> openSet = new();
        HashSet<Tile> visited = new();

        openSet.Enqueue(origin);
        visited.Add(origin);

       
        if (type == PathType.Move 
            || type == PathType.Retreat 
            || type == PathType.Withdraw 
            || type == PathType.Advance
            || type == PathType.Pursue
            || type == PathType.HitAndRun) 
            origin.Cost = 0;
        else if (type == PathType.Attack
            //|| type == PathType.AttackPursue
            ) 
            origin.AttackCost = 0;

        AddToFrontier(origin, type);

        while (openSet.Count > 0)
        {
            Tile currentTile = openSet.Dequeue();

            foreach (Tile adjacentTile in FindAdjacents(currentTile, type))
            {

                //  highlight = adjacentTile;
                //Thread.Sleep(1000); // ms
                //  Update?.Invoke(this, EventArgs.Empty);


                if (!CurrentBoard.IsInside(adjacentTile) || visited.Contains(adjacentTile))
                    continue;

                if (type == PathType.Move 
                    || type == PathType.Retreat
                    || type == PathType.Withdraw
                    || type == PathType.Advance
                    || type == PathType.Pursue
                    || type == PathType.HitAndRun)
                    adjacentTile.Cost = currentTile.Cost + 1;
                else if (type == PathType.Attack
                   // || type == PathType.AttackPursue
                   )
                    adjacentTile.AttackCost = currentTile.AttackCost + 1;

                if (!IsValid(adjacentTile, unit, type))
                    continue;

                if (type == PathType.Move 
                    || type == PathType.Retreat
                    || type == PathType.Withdraw
                    || type == PathType.Advance
                    || type == PathType.Pursue
                    || type == PathType.HitAndRun)
                    adjacentTile.Parent = currentTile;
                else if (type == PathType.Attack
                   // || type == PathType.AttackPursue
                   )
                    adjacentTile.AttackParent = currentTile;

                if (IsPassable(adjacentTile, type))
                {
                    openSet.Enqueue(adjacentTile);
                }

                visited.Add(adjacentTile);

                AddToFrontier(adjacentTile, type);

            }
        }

        Finalize(unit, origin, type);
    }


    List<Tile> FindAdjacents(Tile tile, PathType type)
    {

        if (type == PathType.Retreat
            || type == PathType.Advance
            || type == PathType.Pursue
            || type == PathType.Withdraw)
        {
            List<Tile> adjacentTiles = [];

            if(CurrentAttackPath.TilesInPath.Count >= 2)
            {
                Tile last = CurrentAttackPath.TilesInPath[^1];
                Tile preLast = CurrentAttackPath.TilesInPath[^2];
                Direction retreatDir = GetDirectionFromAdjacentHexes(preLast.Position, last.Position);

                adjacentTiles.Add(CurrentBoard[tile.Position + GetDirectionFromDirection(retreatDir, tile.Position)]);
            }
            else
            {
                MessageController.Instance.Show($"No Attack Path for retreat (path has {CurrentAttackPath.TilesInPath.Count} tiles)");
            }

            return adjacentTiles;
        }

        else
        {
            return tile.Adjacents;
        }
    }


    bool IsValid(Tile tile, Unit unit, PathType type)
    {
        bool valid = false;

        switch (type)
        {
            case PathType.Move:
                if (tile.Cost <= unit.MarchMove
                        && !CurrentFrontier.Tiles.Contains(tile))
                {
                    valid = true;
                }
                break;

            case PathType.HitAndRun:
                if (tile.Cost <= unit.MarchMove-1
                        && !CurrentFrontier.Tiles.Contains(tile))
                {
                    valid = true;
                }
                break;


            case PathType.Retreat:
                if (tile.Cost <= CombatManager.Instance.NumberOfRetreatSpaces
                    && !CurrentFrontier.Tiles.Contains(tile)
                    && !tile.Occupied)
                {
                    valid = true;
                }
                break;

            case PathType.Withdraw:
                if (tile.Cost <= unit.MarchMove-1
                    && !CurrentFrontier.Tiles.Contains(tile)
                    && !tile.Occupied)
                {
                    valid = true;
                }
                break;

            case PathType.Pursue:
                if (tile.Cost <= unit.AttackMove
                    && !CurrentFrontier.Tiles.Contains(tile)
                    && !tile.Occupied)
                {
                    valid = true;
                }
                break;


            case PathType.Advance:
                if (tile.Cost == 1
                    && !CurrentFrontier.Tiles.Contains(tile)
                    && !tile.Occupied)
                {
                    valid = true;
                }
                break;


            case PathType.Attack:
                if (tile.AttackCost <= unit.AttackRange
                    && !CurrentAttackFrontier.Tiles.Contains(tile))
                {
                    valid = true;
                }
                break;

/*
            case PathType.AttackPursue:
                if (tile.AttackCost <= 1
                    && !CurrentAttackFrontier.Tiles.Contains(tile))
                {
                    valid = true;
                }
                break;
*/
        }

        return valid;
    }


    bool IsPassable(Tile tile, PathType type)
    {
        bool valid = false;

        switch (type)
        {
            case PathType.Move:
            case PathType.Retreat:
            case PathType.Advance:
            case PathType.Pursue:
            case PathType.Withdraw:
            case PathType.HitAndRun:
                if (tile.CanBePassed)
                {
                    valid = true;
                }
                break;

            case PathType.Attack:
          //  case PathType.AttackPursue:
                if (tile.CanBeAttackPassed)
                {
                    valid = true;
                }
                break;
        }

        return valid;
    }


    void AddToFrontier(Tile tile, PathType type)
    {
        switch (type)
        {
            case PathType.Move:
            case PathType.Retreat:
            case PathType.Advance:
            case PathType.Pursue:
            case PathType.Withdraw:
            case PathType.HitAndRun:
                CurrentFrontier.Tiles.Add(tile);
                break;

            case PathType.Attack:
          //  case PathType.AttackPursue:
                CurrentAttackFrontier.Tiles.Add(tile);
                break;
        }
    }


    void Finalize(Unit unit, Tile origin, PathType type)
    {

        switch (type)
        {
            case PathType.Move:

                OriginalSpace = origin;

                CurrentFrontier.Tiles = CurrentFrontier.Tiles
                .Take(1)
                .Concat(CurrentFrontier.Tiles.Skip(1).Where(t => !t.Occupied))
                .ToList();

                foreach (Tile tile in CurrentFrontier.Tiles)
                    AddToSpaces(tile, origin);

                FindAttackZone(unit, origin, type);
                break;


            case PathType.HitAndRun:

                OriginalSpace = origin;

                CurrentFrontier.Tiles = CurrentFrontier.Tiles
                .Take(1)
                .Concat(CurrentFrontier.Tiles.Skip(1).Where(t => !t.Occupied))
                .ToList();

                foreach (Tile tile in CurrentFrontier.Tiles)
                    AddToSpaces(tile, origin);
                break;


            case PathType.Retreat:
                if (CurrentFrontier.Tiles.Count > 0)
                {
                    // Find max cost tile efficiently without sorting - O(n) instead of O(n log n)
                    Tile maxCostTile = CurrentFrontier.Tiles[0];
                    foreach (var tile in CurrentFrontier.Tiles)
                    {
                        if (tile.Cost > maxCostTile.Cost)
                        {
                            maxCostTile = tile;
                        }
                    }
                    int numberOfUnresolvedRetreatSpaces = CombatManager.Instance.NumberOfRetreatSpaces - maxCostTile.Cost;

                    var adjacents = FindAdjacents(maxCostTile, PathType.Retreat);
                    if (adjacents.Count > 0 && adjacents.First()?.Unit?.Faction == unit.Faction)
                        numberOfUnresolvedRetreatSpaces = 0;

                    AddToSpaces(maxCostTile, origin);
                    unit.Health.Damage(numberOfUnresolvedRetreatSpaces);
                }
                break;

            case PathType.Pursue:
                CurrentFrontier.Tiles = CurrentFrontier.Tiles
                .Skip(1)
                .ToList();
                foreach (Tile tile in CurrentFrontier.Tiles)
                    AddToSpaces(tile, origin);
                FindAttackZone(unit, origin, type);
                break;

            case PathType.Advance:
                CurrentFrontier.Tiles = CurrentFrontier.Tiles
                .Skip(1)
                .ToList();
                foreach (Tile tile in CurrentFrontier.Tiles)
                    AddToSpaces(tile, origin);
                break;

            case PathType.Withdraw:
                CurrentFrontier.Tiles = CurrentFrontier.Tiles
                .Skip(1)
                .ToList();
                foreach (Tile tile in CurrentFrontier.Tiles)
                    AddToSpaces(tile, origin);
                break;

            case PathType.Attack:
           // case PathType.AttackPursue:
                FindTargets(unit, origin);
                break;

        }

    }

      
    void FindAttackZone(Unit unit, Tile origin,PathType type)
    {

        CurrentEngagements.Clear();

        if (IsEngaged(unit, origin) && type !=PathType.Advance )
        {
            foreach (Tile tile in origin.Adjacents)
            {

                if (tile == null)
                    continue;

                Unit target = tile.Unit;

                if (Board.IsTarget(unit, target))
                {
                    AddToEngagements(tile);
                }
            }
        }


        foreach (Tile tile in CurrentFrontier.Tiles)
        {
            if (tile.Cost <= unit.AttackMove)
            {


               // if (type == PathType.Pursue)
                
             //       FindPaths(unit, tile, PathType.AttackPursue);
                
              //  else

                    FindPaths(unit, tile, PathType.Attack);
            }
        }

    }


    void FindTargets(Unit unit, Tile origin)
    {

        if (CurrentEngagements.Count > 0) //Unit is Engaged...
        {
            foreach (Tile target in CurrentEngagements)
            {
                if (CurrentAttackFrontier.Tiles.Contains(target))
                {
                    AddToTargets(target, origin, unit);
                }
            }
        }

        else
        {

            foreach (Tile tile in CurrentAttackFrontier.Tiles)
            {
                Unit target = tile.Unit;

                if (Board.IsTarget(unit, target))
                {
                    AddToTargets(tile, origin, unit);
                }
            }

        }

    }


    void AddToTargets(Tile tile, Tile origin, Unit unit)
    {
        if (CurrentTargets.ContainsKey(tile))
            return;

        CurrentTargets.Add(tile, (MakePath(origin, unit.Tile, PathType.Move),
                                    MakePath(tile, origin, PathType.Attack)));
    }


    void AddToEngagements(Tile tile)
    {
        if (CurrentEngagements.Contains(tile))
            return;

        CurrentEngagements.Add(tile);
    }


    bool IsEngaged(Unit unit, Tile origin)
    {

        foreach (Tile adjacentTile in origin.Adjacents)
        {
            if (adjacentTile != null
                && adjacentTile.Unit != null
                && adjacentTile.Unit.Faction != unit.Faction)
            {
                return true;
            }
        }

        return false;
    }


    void AddToSpaces(Tile tile, Tile origin)
    {
        if (CurrentSpaces.ContainsKey(tile))
            return;
        CurrentSpaces.Add(tile, MakePath(tile, origin, PathType.Move));
    }


    public void AssignPath(Path path, PathType type)
    {
        if (type == PathType.Move)
            CurrentPath = path;
        else if (type == PathType.Attack)
            CurrentAttackPath = path;
    }


    public void PathBetween(Tile dest, Tile source, PathType type)
    {
        if (type == PathType.Move)
            CurrentPath = MakePath(dest, source, type);
        else if (type == PathType.Attack)
            CurrentAttackPath = MakePath(dest, source, type);

    }


    Path MakePath(Tile destination, Tile origin, PathType type)
    {
        List<Tile> tiles = [];
        Tile current = destination;

        HashSet<Tile> visitedTiles = new HashSet<Tile>();
        int maxIterations = 1000; // Safety limit
        int iterations = 0;

        while (current != origin && iterations < maxIterations)
        {
            if (visitedTiles.Contains(current))
            {
                // Circular reference detected, break to prevent infinite loop
                break;
            }

            tiles.Add(current);
            visitedTiles.Add(current);
            iterations++;

            if (type == PathType.Move || type == PathType.Retreat)
            {
                if (current.Parent == null) break;  // Prevent infinite loop
                current = current.Parent;
            }
            else if (type == PathType.Attack)
            {
                if (current.AttackParent == null) break;  // Prevent infinite loop
                current = current.AttackParent;
            }
            else
            {
                break;
            }
        }

        tiles.Add(origin);
        tiles.Reverse();

        return new Path { TilesInPath = tiles };
    }


    public void Reset(PathType type)
    {

        switch (type)
        {
            case PathType.Move:
            case PathType.Retreat:
            case PathType.Advance:
                CurrentFrontier.Tiles.Clear();
                CurrentPath.TilesInPath.Clear();
                CurrentTargets.Clear();
                CurrentSpaces.Clear();
                break;

            case PathType.Withdraw:
                CurrentSpaces.Clear();
                break;

            case PathType.Attack:
                CurrentAttackFrontier.Tiles.Clear();
                CurrentAttackPath.TilesInPath.Clear();
                break;
        }

    }

    public void ResetAll()
    {
        CurrentFrontier.Tiles.Clear();
        CurrentPath.TilesInPath.Clear();
        CurrentTargets.Clear();
        CurrentSpaces.Clear();
        CurrentAttackFrontier.Tiles.Clear();
        CurrentAttackPath.TilesInPath.Clear();
    }


    public Direction GetDirectionFromAdjacentHexes(Position from, Position to)
    {

        int rowDiff = to.Row - from.Row;
        int colDiff = to.Column - from.Column;
        Direction dir = new Direction(rowDiff, colDiff);

        bool isEvenRow = (from.Row % 2 == 0);

        if (isEvenRow)
        {
            return dir switch
            {
                { RowDelta: -1, ColumnDelta: -1 } => Direction.EvenRowUpperLeft,
                { RowDelta: -1, ColumnDelta: 0 } => Direction.EvenRowUpperRight,
                { RowDelta: 1, ColumnDelta: -1 } => Direction.EvenRowLowerLeft,
                { RowDelta: 1, ColumnDelta: 0 } => Direction.EvenRowLowerRight,
                { RowDelta: 0, ColumnDelta: -1 } => Direction.EvenRowLeft,
                { RowDelta: 0, ColumnDelta: 1 } => Direction.EvenRowRight,
                _ => Direction.None
            };
        }
        else
        {
            return dir switch
            {
                { RowDelta: -1, ColumnDelta: 0 } => Direction.OddRowUpperLeft,
                { RowDelta: -1, ColumnDelta: 1 } => Direction.OddRowUpperRight,
                { RowDelta: 1, ColumnDelta: 0 } => Direction.OddRowLowerLeft,
                { RowDelta: 1, ColumnDelta: 1 } => Direction.OddRowLowerRight,
                { RowDelta: 0, ColumnDelta: -1 } => Direction.OddRowLeft,
                { RowDelta: 0, ColumnDelta: 1 } => Direction.OddRowRight,
                _ => Direction.None
            };
        }
    }


    public Direction GetDirectionFromDirection(Direction dir, Position pos)
    {

        if (pos.Row % 2 == 0)
        {
            if (dir == Direction.OddRowLeft) return Direction.EvenRowLeft;
            if (dir == Direction.OddRowUpperLeft) return Direction.EvenRowUpperLeft;
            if (dir == Direction.OddRowUpperRight) return Direction.EvenRowUpperRight;
            if (dir == Direction.OddRowRight) return Direction.EvenRowRight;
            if (dir == Direction.OddRowLowerRight) return Direction.EvenRowLowerRight;
            if (dir == Direction.OddRowLowerLeft) return Direction.EvenRowLowerLeft;
            else
                return dir;
        }
        else
        {

            if (dir == Direction.EvenRowLeft) return Direction.OddRowLeft;
            if (dir == Direction.EvenRowUpperLeft) return Direction.OddRowUpperLeft;
            if (dir == Direction.EvenRowUpperRight) return Direction.OddRowUpperRight;
            if (dir == Direction.EvenRowRight) return Direction.OddRowRight;
            if (dir == Direction.EvenRowLowerRight) return Direction.OddRowLowerRight;
            if (dir == Direction.EvenRowLowerLeft) return Direction.OddRowLowerLeft;
            else
                return dir;
        }


    }

}

