using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BoLLogic;


public enum GamePhase
{
    Select,
    Order,
    Turn,
    End
}


public sealed class GameManager
{

    private static readonly Lazy<GameManager> instance = new Lazy<GameManager>(() => new GameManager());

    public static GameManager Instance => instance.Value;


    public Board CurrentBoard { get; set; }

    public int NumberOfRows { get; set; }
    public int NumberOfColumns { get; set; }

    public Dictionary<(int, int), string> Tiles { get; set; }
    public Dictionary<(int, int), (string Faction, string Unit, string State, string Health)> Units { get; set; }
    public List<(string Faction, string Card, string State)> Cards { get; set; }
    public List<(string Faction, int Morale, int maxHand, int Hand, int maxAction, int Action)> Players { get; set; }
    public string CurrentPlayer { get; set; }
    public int EndRound { get; set; }
    public string CurrentGamePhase { get; set; }
    public string CurrentTurnPhase { get; set; }
    public int CurrentGameRound { get; set; }


    public void LoadGame(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Scenario file not found: {path}");
        }

        string json = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidDataException($"Scenario file is empty: {path}");
        }

        GameData data = JsonSerializer.Deserialize<GameData>(json);

        if (data == null)
        {
            throw new InvalidDataException($"Failed to deserialize game data from: {path}");
        }

        // Validate required data
        if (data.Tiles == null || data.Units == null || data.Cards == null || data.Players == null)
        {
            throw new InvalidDataException($"Game data is incomplete or corrupted: {path}");
        }

        NumberOfRows = data.NumberOfRows;
        NumberOfColumns = data.NumberOfColumns;
        Players = data.Players.Select(p => (p.Faction, p.Morale, p.MaxHand, p.Hand, p.MaxAction, p.Action)).ToList();
        Tiles = data.Tiles.ToDictionary(
            kv => ParseKey(kv.Key),
            kv => kv.Value
        );
        Units = data.Units.ToDictionary(
            kv => ParseKey(kv.Key),
            kv => (kv.Value.Faction, kv.Value.Unit, kv.Value.State, kv.Value.Health)
        );
        Cards = data.Cards.Select(c => (c.Faction, c.Card, c.State)).ToList();
        CurrentPlayer = data.CurrentPlayer;
        EndRound = data.EndRound;
        CurrentGameRound = data.CurrentGameRound;
        CurrentGamePhase = data.CurrentGamePhase;
        CurrentTurnPhase = data.CurrentTurnPhase;
    }

    private (int, int) ParseKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or empty");
        }

        var parts = key.Trim('(', ')').Split(',');

        if (parts.Length != 2)
        {
            throw new FormatException($"Invalid key format: {key}. Expected format: (row,col)");
        }

        if (!int.TryParse(parts[0], out int row) || !int.TryParse(parts[1], out int col))
        {
            throw new FormatException($"Invalid key format: {key}. Row and column must be integers");
        }

        return (row, col);
    }


    public void SaveGame(string path)
    {

        var gameData = new GameData
        {
            NumberOfRows = NumberOfRows,
            NumberOfColumns = NumberOfColumns,
            Players = CurrentBoard.Players.Select(p => new PlayerData
            {
                Faction = p.Type.ToString(),
                Morale = p.Morale.MoraleValue,
                MaxHand = p.Hand.MaxHand,
                Hand = p.Hand.HandValue,
                MaxAction = p.Action.MaxAction,
                Action = p.Action.ActionValue
            }).ToList(),

            Tiles = CurrentBoard.Tiles.ToDictionary(
                kv => $"({kv.Position.Row},{kv.Position.Column})",
                kv => kv.Type.ToString()),

            Units = CurrentBoard.Units.ToDictionary(
                kv => $"({kv.Position.Row},{kv.Position.Column})",
                kv => new UnitData
                {
                    Faction = kv.Faction.ToString(),
                    Unit = kv.Type.ToString(),
                    State = kv.State.ToString(),
                    Health = kv.Health.GetHealth().ToString(),
                }),

            Cards = CurrentBoard.Cards.Select(c => new CardData
            {
                Faction = c.Faction.ToString(),
                Card = c.Type.ToString(),
                State = c.State.ToString()
            }).ToList(),

            CurrentPlayer = CurrentPlayer,
            EndRound = EndRound,
            CurrentGameRound = CurrentGameRound,
            CurrentGamePhase = CurrentGamePhase,
            CurrentTurnPhase = CurrentTurnPhase
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(gameData, options);
        File.WriteAllText(path, json);
    }




    public void On_GameRoundChanged(object sender, EventArgs e)
    {
        CurrentGameRound = TurnManager.Instance.CurrentGameRound;

        if (CurrentGameRound >= CurrentBoard.EndRound)
        {
            MessageController.Instance.Show("GAME OVER ! Maximum rounds reached!");
            TurnManager.Instance.CurrentGamePhase = GamePhase.End;
        }

        if (CurrentGameRound % 2 == 1)
        {

            foreach (Unit unit in CurrentBoard.Units)
            {
                unit.State = UnitState.Idle;
            }

        }

        if (CurrentGameRound % 4 == 1 || CurrentGameRound % 4 == 2)
        {
            TurnManager.Instance.CurrentPlayer = CurrentBoard.InitialPlayer;
        }
        else
        {
            TurnManager.Instance.CurrentPlayer = CurrentBoard.InitialPlayer.Opponent();
        }

        foreach (Player player in CurrentBoard.Players)
        {
            player.Action.ActionValue = 0;
        }
    }

}








