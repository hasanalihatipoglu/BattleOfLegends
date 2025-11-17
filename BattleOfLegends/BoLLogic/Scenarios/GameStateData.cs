using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoLLogic;

public class GameStateData
{
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
}
