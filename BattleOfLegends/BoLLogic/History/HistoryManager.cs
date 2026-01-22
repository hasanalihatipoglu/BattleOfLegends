using System.Text;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoLLogic;

/// <summary>
/// Manages the game history for undo/redo functionality
/// Snapshots are only taken at turn boundaries for simplicity and reliability
/// </summary>
public sealed class HistoryManager
{
    private static readonly Lazy<HistoryManager> instance = new Lazy<HistoryManager>(() => new HistoryManager());
    public static HistoryManager Instance => instance.Value;

    // Complete history of all actions (for notation display)
    private readonly List<GameAction> _completeHistory = new List<GameAction>();

    // Turn-based snapshots: each snapshot represents the start of a turn
    private readonly Stack<(GameStateSnapshot snapshot, int turnNumber, PlayerType player)> _undoSnapshots = new();
    private readonly Stack<(GameStateSnapshot snapshot, int turnNumber, PlayerType player)> _redoSnapshots = new();

    private Board _board;
    private bool _isUndoingOrRedoing = false;
    private int _currentTurnNumber = 0;

    public bool CanUndo => _undoSnapshots.Count > 1; // Need at least 2 snapshots to undo (current + previous)
    public bool CanRedo => _redoSnapshots.Count > 0;
    public bool IsUndoingOrRedoing => _isUndoingOrRedoing;

    public int UndoStackCount => _undoSnapshots.Count - 1; // -1 because initial state doesn't count as undoable
    public int RedoStackCount => _redoSnapshots.Count;

    private HistoryManager()
    {
    }

    /// <summary>
    /// Initialize the history manager with the game board
    /// </summary>
    public void Initialize(Board board)
    {
        _board = board;
        _currentTurnNumber = 0;

        // Clear any existing state first
        _undoSnapshots.Clear();
        _redoSnapshots.Clear();
        _completeHistory.Clear();

        // Capture initial game state (Turn 0 - before any player acts)
        var initialSnapshot = GameStateSnapshot.Capture(board);
        _undoSnapshots.Push((initialSnapshot, 0, TurnManager.Instance.CurrentPlayer));

        System.Diagnostics.Debug.WriteLine($"[History] Initialized with Turn 0 snapshot ({TurnManager.Instance.CurrentPlayer}'s turn)");
    }

    /// <summary>
    /// Record a new action in the history
    /// Only EndTurnAction triggers a new snapshot (turn boundary)
    /// </summary>
    public void RecordAction(GameAction action)
    {
        // Don't record actions that happen during undo/redo
        if (_isUndoingOrRedoing)
            return;

        // Always add to complete history for notation purposes
        _completeHistory.Add(action);

        // Only capture snapshot when a turn ends (EndTurnAction)
        // This creates a snapshot at the START of the new player's turn
        if (action is EndTurnAction endTurnAction)
        {
            _currentTurnNumber++;

            // Capture state AFTER the end turn action (start of new player's turn)
            var snapshot = GameStateSnapshot.Capture(_board);
            _undoSnapshots.Push((snapshot, _currentTurnNumber, endTurnAction.NewPlayer));

            // Clear redo when new action is performed
            _redoSnapshots.Clear();

            System.Diagnostics.Debug.WriteLine($"[History] Turn {_currentTurnNumber} snapshot captured ({endTurnAction.NewPlayer}'s turn starting)");
        }

        System.Diagnostics.Debug.WriteLine($"[History] Recorded: {action.GetNotation()}");
    }

    /// <summary>
    /// Get the notation of the next turn that would be undone
    /// </summary>
    public string PeekUndoAction()
    {
        if (!CanUndo)
            return null;

        var current = _undoSnapshots.Peek();
        return $"Turn {current.turnNumber} ({current.player}'s turn)";
    }

    /// <summary>
    /// Undo to the previous turn's start
    /// </summary>
    public (bool success, string description) Undo()
    {
        if (!CanUndo || _board == null)
            return (false, null);

        _isUndoingOrRedoing = true;
        try
        {
            // Pop current turn state
            var currentTurn = _undoSnapshots.Pop();

            // Peek at the previous turn (don't pop - we need it as reference)
            var previousTurn = _undoSnapshots.Peek();

            // Restore to previous turn's state
            previousTurn.snapshot.Restore(_board);

            // Push to redo stack
            _redoSnapshots.Push(currentTurn);

            _currentTurnNumber = previousTurn.turnNumber;

            string description = $"Undid to Turn {previousTurn.turnNumber} ({previousTurn.player}'s turn)";
            System.Diagnostics.Debug.WriteLine($"[History] {description}");
            return (true, description);
        }
        finally
        {
            _isUndoingOrRedoing = false;
        }
    }

    /// <summary>
    /// Get the notation of the next turn that would be redone
    /// </summary>
    public string PeekRedoAction()
    {
        if (!CanRedo)
            return null;

        var next = _redoSnapshots.Peek();
        return $"Turn {next.turnNumber} ({next.player}'s turn)";
    }

    /// <summary>
    /// Redo to the next turn's start
    /// </summary>
    public (bool success, string description) Redo()
    {
        if (!CanRedo || _board == null)
            return (false, null);

        _isUndoingOrRedoing = true;
        try
        {
            // Pop from redo stack
            var nextTurn = _redoSnapshots.Pop();

            // Restore the next turn's state
            nextTurn.snapshot.Restore(_board);

            // Push back to undo stack
            _undoSnapshots.Push(nextTurn);

            _currentTurnNumber = nextTurn.turnNumber;

            string description = $"Redid to Turn {nextTurn.turnNumber} ({nextTurn.player}'s turn)";
            System.Diagnostics.Debug.WriteLine($"[History] {description}");
            return (true, description);
        }
        finally
        {
            _isUndoingOrRedoing = false;
        }
    }

    /// <summary>
    /// Get the complete game history in chess-like notation
    /// </summary>
    public string GetHistoryNotation()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Battle of Legends - Game History ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        int moveNumber = 1;
        foreach (var action in _completeHistory)
        {
            sb.AppendLine($"{moveNumber}. {action.GetNotation()}");
            moveNumber++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Save the complete game history to a file
    /// </summary>
    public void SaveHistoryToFile(string filePath)
    {
        try
        {
            string history = GetHistoryNotation();
            File.WriteAllText(filePath, history);
            System.Diagnostics.Debug.WriteLine($"[History] Saved to: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[History] Error saving to file: {ex.Message}");
        }
    }

    /// <summary>
    /// Auto-save history to the default location (both text and JSON formats)
    /// </summary>
    public void AutoSaveHistory()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string directory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BattleOfLegends"
        );

        // Ensure directory exists
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Save human-readable text version
        string textPath = System.IO.Path.Combine(directory, $"game_history_{timestamp}.txt");
        SaveHistoryToFile(textPath);

        // Save machine-readable JSON version
        string jsonPath = System.IO.Path.Combine(directory, $"game_save_{timestamp}.json");
        SaveHistoryToJson(jsonPath);
    }

    /// <summary>
    /// Save history as JSON for loading across sessions
    /// </summary>
    private void SaveHistoryToJson(string filePath)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            string json = JsonSerializer.Serialize(_completeHistory, options);
            File.WriteAllText(filePath, json);
            System.Diagnostics.Debug.WriteLine($"[History] Saved JSON to: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[History] Error saving JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Load history from JSON file
    /// </summary>
    public List<GameAction> LoadHistoryFromJson(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            string json = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };

            var actions = JsonSerializer.Deserialize<List<GameAction>>(json, options);
            System.Diagnostics.Debug.WriteLine($"[History] Loaded {actions?.Count ?? 0} actions from JSON");

            return actions;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[History] Error loading JSON: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get the most recent JSON save file
    /// </summary>
    public string GetMostRecentJsonSaveFile()
    {
        string directory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BattleOfLegends"
        );

        if (!System.IO.Directory.Exists(directory))
            return null;

        var files = System.IO.Directory.GetFiles(directory, "game_save_*.json");
        if (files.Length == 0)
            return null;

        return files.OrderByDescending(f => f).FirstOrDefault();
    }

    /// <summary>
    /// Clear all history (but keep initial snapshot)
    /// </summary>
    public void Clear()
    {
        _redoSnapshots.Clear();
        _completeHistory.Clear();
        _currentTurnNumber = 0;

        // Re-capture initial state
        if (_board != null)
        {
            _undoSnapshots.Clear();
            var initialSnapshot = GameStateSnapshot.Capture(_board);
            _undoSnapshots.Push((initialSnapshot, 0, TurnManager.Instance.CurrentPlayer));
        }

        System.Diagnostics.Debug.WriteLine("[History] Cleared all history");
    }

    /// <summary>
    /// Get the last action notation for display
    /// </summary>
    public string GetLastActionNotation()
    {
        if (_completeHistory.Count == 0)
            return "No actions yet";

        return _completeHistory[^1].GetNotation();
    }

    /// <summary>
    /// Get the next redo action notation for display
    /// </summary>
    public string GetNextRedoNotation()
    {
        if (_redoSnapshots.Count == 0)
            return "No turns to redo";

        var next = _redoSnapshots.Peek();
        return $"Turn {next.turnNumber} ({next.player})";
    }

    /// <summary>
    /// Get the most recent save file path
    /// </summary>
    public string GetMostRecentSaveFile()
    {
        string directory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BattleOfLegends"
        );

        if (!System.IO.Directory.Exists(directory))
            return null;

        var files = System.IO.Directory.GetFiles(directory, "game_history_*.txt");
        if (files.Length == 0)
            return null;

        return files.OrderByDescending(f => f).FirstOrDefault();
    }

    /// <summary>
    /// Get a copy of the complete history for replay purposes
    /// </summary>
    public List<GameAction> GetCompleteHistory()
    {
        return new List<GameAction>(_completeHistory);
    }

    /// <summary>
    /// Get the current turn number
    /// </summary>
    public int CurrentTurnNumber => _currentTurnNumber;
}
