using System.Text.Json;

namespace BoLLogic;

public class Board
{

    public Position this[int row, int col]
    {
        get { return Positions[row, col]; }
        set { Positions[row, col] = value; }
    }

    public Tile this[Position pos]
    {
        get { return Tiles.Find(x => x.Position == pos); }
        set { Tiles.Add(value); }
    }


    public Position[,] Positions { get; set; }
    public List<Unit> Units { get; set; } = [];
    public List<Tile> Tiles { get; set; } = [];
    public List<Card> Cards { get; set; } = [];
    public List<Player> Players { get; set; } = []; 
    public PlayerType CurrentPlayer { get; set; }
    public GamePhase GamePhase { get; set; }
    public TurnPhase TurnPhase { get; set; }
    public int EndRound { get; set; }
    public int GameRound { get; set; }


    public void Initialize()
    {
        System.Diagnostics.Debug.WriteLine("========== BOARD INITIALIZATION STARTED ==========");
        AddPlayers();
        SetFirstPlayer();
        AddPositions();
        AddTiles();
        AddUnits();
        AddCards();
        System.Diagnostics.Debug.WriteLine($"========== ADDED {Cards.Count} CARDS ==========");
        SetTiles();
        SetLeaders();
        SetRound();
        SetPhases();

        // Trigger initial turn phase event to update card states
        System.Diagnostics.Debug.WriteLine("========== TRIGGERING AdvanceTurnPhase ==========");
        TurnManager.Instance.AdvanceTurnPhase();
        System.Diagnostics.Debug.WriteLine("========== BOARD INITIALIZATION COMPLETE ==========");
    }


    void AddPlayers()
    {
        foreach (var player in GameManager.Instance.Players)
        {
            if (!Enum.TryParse(player.Faction, out PlayerType playerType))
            {
                System.Diagnostics.Debug.WriteLine($"Invalid faction: {player.Faction}");
                continue;
            }

            var newPlayer = new Player(playerType)
            {
                Morale = new MoraleSystem { MoraleValue = player.Morale },
                Hand = new HandSystem { MaxHand = player.maxHand, HandValue = player.Hand },
                Action = new ActionSystem { MaxAction = player.maxAction, ActionValue = player.Action }
            };

            Players.Add(newPlayer);
        }
    }


    void SetFirstPlayer()
    {
        if (Enum.TryParse(GameManager.Instance.CurrentPlayer, out PlayerType playerType))
        {
            CurrentPlayer = playerType;
            TurnManager.Instance.CurrentPlayer = CurrentPlayer;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Invalid first player faction: {GameManager.Instance.CurrentPlayer}");
        }
    }


    void AddPositions()
    {
        Positions = new Position[GameManager.Instance.NumberOfRows, GameManager.Instance.NumberOfColumns];

        for (int r = 0; r < Positions.GetLength(0); r++)
        {
            for (int c = 0; c < Positions.GetLength(1); c++)
            {
                Positions[r, c] = new Position(r, c);
            }
        }
    }


    void AddTiles()
    {
        for (int r = 0; r < Positions.GetLength(0); r++)
        {
            for (int c = 0; c < Positions.GetLength(1); c++)
            {
                string tileTypeName = GameManager.Instance.Tiles.TryGetValue((r, c), out var name) ? name : "Grass";

                string fullTypeName = $"BoLLogic.{tileTypeName}"; // Replace with your actual namespace

                Type type = Type.GetType(fullTypeName);
                if (type == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Tile class not found: {fullTypeName}, defaulting to Grass");
                    type = typeof(Grass);
                }

                try
                {
                    var instance = Activator.CreateInstance(type) as Tile;
                    if (instance != null)
                    {
                        instance.Position = Positions[r, c];
                        Tiles.Add(instance);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not instantiate tile: {tileTypeName}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating tile '{tileTypeName}': {ex.Message}");
                }
            }
        }
    }


    void SetTiles()
    {
        foreach (Tile tile in Tiles)
        {
            SetTile(tile);
        }
    }


    void SetTile(Tile tile)
    {

        tile.Adjacents = FindAdjacentTiles(tile);

        foreach (Unit unit in Units)
        {
            if (tile.Position == unit.Position)
            {
                tile.Unit = unit;
                unit.Tile = tile;

                tile.Occupied = true;
            }
        }
    }


    void SetLeaders()
    {
        foreach (Unit unit in Units)
        {
            if (unit.Type == UnitType.Leader)
            {
                foreach(Player player in Players)
                {
                    if(player.Type == unit.Faction)
                    {
                        player.Leader = unit;
                    }
                }                
            }
        }
    }


    void AddUnits()
    {
        for (int r = 0; r < Positions.GetLength(0); r++)
        {
            for (int c = 0; c < Positions.GetLength(1); c++)
            {
                if (!GameManager.Instance.Units.TryGetValue((r, c), out var unitData))
                    continue;

                var (faction, unitTypeName, unitState, unitHealth) = unitData;

                if (!Enum.TryParse(faction, out PlayerType playerType))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid faction: {faction}");
                    continue;
                }

                if (!Enum.TryParse(unitState, out UnitState state))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid unit state: {unitState}");
                    continue;
                }

                if (!Int32.TryParse(unitHealth, out int health))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid unit health: {unitHealth}");
                    continue;
                }

                string fullTypeName = $"BoLLogic.{unitTypeName}"; // Replace 'YourNamespace' with actual namespace

                Type type = Type.GetType(fullTypeName);
                if (type == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Unit class not found: {fullTypeName}");
                    continue;
                }

                try
                {
                    var instance = Activator.CreateInstance(type, playerType) as Unit;
                    if (instance != null)
                    {
                        instance.Position = Positions[r, c];
                        instance.State = state;
                        instance.Health = new HealthSystem { Unit = instance };
                        instance.Health.SetHealth(health);

                        // Subscribe to combat events for automatic retreat
                        CombatManager.Instance.ChangeUnitState += instance.On_StateChanged;

                        Units.Add(instance);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to create unit: {unitTypeName}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating unit '{unitTypeName}': {ex.Message}");
                }
            }
        }
    }


    void AddCards()
    {
        foreach (var (faction, cardName, cardState) in GameManager.Instance.Cards)
        {
            if (!Enum.TryParse(faction, out PlayerType playerType))
            {
                System.Diagnostics.Debug.WriteLine($"Invalid faction: {faction}");
                continue;
            }

            if (!Enum.TryParse(cardState, out CardState state))
            {
                System.Diagnostics.Debug.WriteLine($"Invalid faction: {cardState}");
                continue;
            }

            // Assuming all card classes are in the same namespace
            string fullTypeName = $"BoLLogic.{cardName}"; // Replace 'YourNamespace' with actual one

            Type type = Type.GetType(fullTypeName);
            if (type == null)
            {
                System.Diagnostics.Debug.WriteLine($"Card class not found: {fullTypeName}");
                continue;
            }

            // Create instance with constructor that takes PlayerType
            try
            {
                var instance = Activator.CreateInstance(type, playerType) as Card;
                if (instance != null)
                {
                    instance.State = state;

                    // Subscribe to turn phase changes to update card playability
                    TurnManager.Instance.ChangeTurnPhase += instance.On_Update;
                    System.Diagnostics.Debug.WriteLine($"Subscribed card: {instance.Type} ({instance.Faction}) with state {instance.State}");

                    Cards.Add(instance);
                }

                else
                    System.Diagnostics.Debug.WriteLine($"Failed to create card: {cardName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating card '{cardName}': {ex.Message}");
            }
        }
    }


    void SetRound()
    {
        EndRound = GameManager.Instance.EndRound;
        GameRound = GameManager.Instance.CurrentGameRound;
        TurnManager.Instance.CurrentGameRound = GameRound;
    }


    void SetPhases()
    {
        if (Enum.TryParse(GameManager.Instance.CurrentGamePhase, out GamePhase gphase))
        {
            GamePhase = gphase;
            TurnManager.Instance.CurrentGamePhase = GamePhase;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Invalid game phase: {GameManager.Instance.CurrentGamePhase}");
        }

        if (Enum.TryParse(GameManager.Instance.CurrentTurnPhase, out TurnPhase tphase))
        {
            TurnPhase = tphase;
            TurnManager.Instance.CurrentTurnPhase = TurnPhase;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Invalid turn phase: {GameManager.Instance.CurrentTurnPhase}");
        }
    }



    List<Tile> FindAdjacentTiles(Tile tile)
    {

        List<Tile> adjacentTiles = [];

        if (tile.Position.Row % 2 == 0)
        {
            adjacentTiles.Add(this[tile.Position + Direction.EvenRowLeft]);
            adjacentTiles.Add(this[tile.Position + Direction.EvenRowUpperLeft]);
            adjacentTiles.Add(this[tile.Position + Direction.EvenRowUpperRight]);
            adjacentTiles.Add(this[tile.Position + Direction.EvenRowRight]);
            adjacentTiles.Add(this[tile.Position + Direction.EvenRowLowerRight]);
            adjacentTiles.Add(this[tile.Position + Direction.EvenRowLowerLeft]);
        }

        else
        {
            adjacentTiles.Add(this[tile.Position + Direction.OddRowLeft]);
            adjacentTiles.Add(this[tile.Position + Direction.OddRowUpperLeft]);
            adjacentTiles.Add(this[tile.Position + Direction.OddRowUpperRight]);
            adjacentTiles.Add(this[tile.Position + Direction.OddRowRight]);
            adjacentTiles.Add(this[tile.Position + Direction.OddRowLowerRight]);
            adjacentTiles.Add(this[tile.Position + Direction.OddRowLowerLeft]);
        }

        return adjacentTiles;
    }


    public bool IsInside(Tile tile)
    {
        if (tile == null) return false;

        if (tile.Position.Row % 2 == 0)
            return tile.Position.Row >= 0
                && tile.Position.Row < Positions.GetLength(0)
                && tile.Position.Column >= 0
                && tile.Position.Column < Positions.GetLength(1);
        else
            return tile.Position.Row >= 0
                && tile.Position.Row < Positions.GetLength(0)
                && tile.Position.Column >= 0
                && tile.Position.Column < Positions.GetLength(1) - 1;
    }



    public bool IsOccupied(Tile tile)
    {
        return tile != null && tile.Occupied;
    }

    public static bool IsTarget(Unit unit, Unit target)
    {
        return target != null && unit.Faction != target.Faction;
    }

    public int GetRowNumber()
    {
        return Positions.GetLength(0);
    }

    public int GetColumnNumber()
    {
        return Positions.GetLength(1);
    }

}


