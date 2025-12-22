using System.Text;
using System.IO;

namespace BoLLogic;

/// <summary>
/// Manages the game history for undo/redo functionality
/// </summary>
public sealed class HistoryManager
{
    private static readonly Lazy<HistoryManager> instance = new Lazy<HistoryManager>(() => new HistoryManager());
    public static HistoryManager Instance => instance.Value;

    private readonly Stack<GameAction> _undoStack = new Stack<GameAction>();
    private readonly Stack<GameAction> _redoStack = new Stack<GameAction>();
    private readonly List<GameAction> _completeHistory = new List<GameAction>();

    private Board _board;
    private bool _isUndoingOrRedoing = false; // Flag to prevent recording during undo/redo

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public bool IsUndoingOrRedoing => _isUndoingOrRedoing;

    public int UndoStackCount => _undoStack.Count;
    public int RedoStackCount => _redoStack.Count;

    private HistoryManager()
    {
    }

    /// <summary>
    /// Initialize the history manager with the game board
    /// </summary>
    public void Initialize(Board board)
    {
        _board = board;
    }

    /// <summary>
    /// Record a new action in the history
    /// </summary>
    public void RecordAction(GameAction action)
    {
        // Don't record actions that happen during undo/redo
        if (_isUndoingOrRedoing)
            return;

        _undoStack.Push(action);
        _completeHistory.Add(action);

        // Clear redo stack when a new action is performed
        _redoStack.Clear();

        System.Diagnostics.Debug.WriteLine($"[History] Recorded: {action.GetNotation()}");
    }

    /// <summary>
    /// Get the notation of the next action that would be undone
    /// </summary>
    public string PeekUndoAction()
    {
        if (!CanUndo)
            return null;

        return _undoStack.Peek().GetNotation();
    }

    /// <summary>
    /// Undo the last action
    /// </summary>
    /// <returns>Tuple of (success, actionDescription)</returns>
    public (bool success, string description) Undo()
    {
        if (!CanUndo || _board == null)
            return (false, null);

        _isUndoingOrRedoing = true;
        try
        {
            var action = _undoStack.Pop();
            string description = action.GetNotation();
            bool success = action.Undo(_board);

            if (success)
            {
                _redoStack.Push(action);
                System.Diagnostics.Debug.WriteLine($"[History] Undid: {description}");
            }
            else
            {
                // If undo failed, put it back on the undo stack
                _undoStack.Push(action);
                System.Diagnostics.Debug.WriteLine($"[History] Failed to undo: {description}");
            }

            return (success, description);
        }
        finally
        {
            _isUndoingOrRedoing = false;
        }
    }

    /// <summary>
    /// Get the notation of the next action that would be redone
    /// </summary>
    public string PeekRedoAction()
    {
        if (!CanRedo)
            return null;

        return _redoStack.Peek().GetNotation();
    }

    /// <summary>
    /// Redo the last undone action
    /// </summary>
    /// <returns>Tuple of (success, actionDescription)</returns>
    public (bool success, string description) Redo()
    {
        if (!CanRedo || _board == null)
            return (false, null);

        _isUndoingOrRedoing = true;
        try
        {
            var action = _redoStack.Pop();
            string description = action.GetNotation();
            bool success = action.Execute(_board);

            if (success)
            {
                _undoStack.Push(action);
                System.Diagnostics.Debug.WriteLine($"[History] Redid: {description}");
            }
            else
            {
                // If redo failed, put it back on the redo stack
                _redoStack.Push(action);
                System.Diagnostics.Debug.WriteLine($"[History] Failed to redo: {description}");
            }

            return (success, description);
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
    /// Auto-save history to the default location
    /// </summary>
    public void AutoSaveHistory()
    {
        string defaultPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BattleOfLegends",
            $"game_history_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        );

        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(defaultPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        SaveHistoryToFile(defaultPath);
    }

    /// <summary>
    /// Clear all history
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        _completeHistory.Clear();
        System.Diagnostics.Debug.WriteLine("[History] Cleared all history");
    }

    /// <summary>
    /// Get the last action notation for display
    /// </summary>
    public string GetLastActionNotation()
    {
        if (_undoStack.Count == 0)
            return "No actions yet";

        return _undoStack.Peek().GetNotation();
    }

    /// <summary>
    /// Get the next redo action notation for display
    /// </summary>
    public string GetNextRedoNotation()
    {
        if (_redoStack.Count == 0)
            return "No actions to redo";

        return _redoStack.Peek().GetNotation();
    }
}
