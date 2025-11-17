using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoLLogic;

public class GameData
{
    public int NumberOfRows { get; set; }
    public int NumberOfColumns { get; set; }
    public List<PlayerData> Players { get; set; }
    public Dictionary<string, string> Tiles { get; set; }
    public Dictionary<string, UnitData> Units { get; set; }
    public List<CardData> Cards { get; set; }
    public string CurrentPlayer { get; set; }
    public int EndRound { get; set; }
    public int CurrentGameRound { get; set; }
    public string CurrentGamePhase { get; set; }
    public string CurrentTurnPhase { get; set; }
}


public class PlayerData
{
    public string Faction { get; set; }
    public int Morale { get; set; }
    public int MaxHand { get; set; }
    public int Hand { get; set; }
    public int MaxAction { get; set; }
    public int Action { get; set; }
}

public class UnitData
{
    public string Faction { get; set; }
    public string Unit { get; set; }
    public string State { get; set; }
    public string Health { get; set; }
}

public class CardData
{
    public string Faction { get; set; }
    public string Card { get; set; }
    public string State { get; set; }
}

