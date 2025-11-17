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

    private static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }
            return instance;
        }
    }


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
        string json = File.ReadAllText(path);
        GameData data =  JsonSerializer.Deserialize<GameData>(json);

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
        var parts = key.Trim('(', ')').Split(',');
        return (int.Parse(parts[0]), int.Parse(parts[1]));
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

        if (CurrentGameRound == CurrentBoard.EndRound)
        {
            MessageController.Instance.Show("GAME OVER !");
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
            TurnManager.Instance.CurrentPlayer = CurrentBoard.CurrentPlayer;
        }
        else
        {
            TurnManager.Instance.CurrentPlayer = CurrentBoard.CurrentPlayer.Opponent();
        }

        foreach (Player player in CurrentBoard.Players)
        {
            player.Action.ActionValue = 0;
        }
    }

}








