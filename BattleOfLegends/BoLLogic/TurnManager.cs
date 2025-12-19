using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BoLLogic;


public enum TurnPhase
{
    Move,
    Attack,
    Defend,
    Roll,
    Counter,
    Advance,
    Form,
    None
}


public sealed class TurnManager
{

    private static readonly Lazy<TurnManager> instance = new Lazy<TurnManager>(() => new TurnManager());

    public static TurnManager Instance => instance.Value;


    public Unit SelectedUnit { get; set; } 
    public Card SelectedCard { get; set; }
    public PlayerType CurrentPlayer { get; set; }
    public TurnPhase CurrentTurnPhase { get; set; }
    public GamePhase CurrentGamePhase { get; set; }
    public int CurrentGameRound { get; set; } = 1;



    public event EventHandler ChangeTurnPhase;
    public event EventHandler ChangePlayer;
    public event EventHandler ChangeGamePhase;
    public event EventHandler ChangeGameRound;
    public event EventHandler<HandEventArgs> ChangeHand;
    public event EventHandler<ActionEventArgs> ChangeAction;


    public void AdvanceTurnPhase(int i)
    {
        int currentPhaseValue = (int)CurrentTurnPhase;
        int totalPhases = System.Enum.GetValues(typeof(TurnPhase)).Length;

        // Advance phase safely with wrap-around
        currentPhaseValue = (currentPhaseValue + i) % totalPhases;

        // Ensure non-negative value
        if (currentPhaseValue < 0)
        {
            currentPhaseValue += totalPhases;
        }

        CurrentTurnPhase = (TurnPhase)currentPhaseValue;

        switch (CurrentTurnPhase)
        {
            case TurnPhase.Move:
                ChangeCurrentPlayer();
                break;

            case TurnPhase.Attack:
                break;

            case TurnPhase.Defend:
                ChangeCurrentPlayer();
                break;

            case TurnPhase.Roll:
                ChangeCurrentPlayer();
                break;

            case TurnPhase.Counter:
                ChangeCurrentPlayer();
                break;

            case TurnPhase.Advance:
                ChangeCurrentPlayer();
                break;

            case TurnPhase.Form:
                break;
        }

        ChangeTurnPhase?.Invoke(this, EventArgs.Empty);
    }


    public void AdvanceTurnPhase()
    {
        // System.Diagnostics.Debug.WriteLine($"AdvanceTurnPhase called. Current phase: {CurrentTurnPhase}, Current player: {CurrentPlayer}");
        // System.Diagnostics.Debug.WriteLine($"ChangeTurnPhase event has {ChangeTurnPhase?.GetInvocationList().Length ?? 0} subscribers");
        ChangeTurnPhase?.Invoke(this, EventArgs.Empty);
    }




    public void ChangeCurrentPlayer()
    {
        PlayerType previousPlayer = CurrentPlayer;
        CurrentPlayer = CurrentPlayer.Opponent();

        // Record player change in history
        HistoryManager.Instance.RecordAction(
            new PlayerChangeAction(previousPlayer, CurrentPlayer)
        );

        ChangePlayer?.Invoke(this, EventArgs.Empty);
    }

    public void SetCurrentPlayer(PlayerType player)
    {
        // System.Diagnostics.Debug.WriteLine($"SetCurrentPlayer called: trying to set to {player}, current is {CurrentPlayer}");
        if (CurrentPlayer != player)
        {
            PlayerType previousPlayer = CurrentPlayer;
            CurrentPlayer = player;

            // Record player change in history
            HistoryManager.Instance.RecordAction(
                new PlayerChangeAction(previousPlayer, CurrentPlayer)
            );

            // System.Diagnostics.Debug.WriteLine($"Player changed to {CurrentPlayer}, raising ChangePlayer event");
            ChangePlayer?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // System.Diagnostics.Debug.WriteLine($"Player is already {player}, not changing");
        }
    }

    /// <summary>
    /// Trigger the ChangePlayer event without recording in history (for undo/redo)
    /// </summary>
    public void TriggerPlayerChangeEvent()
    {
        ChangePlayer?.Invoke(this, EventArgs.Empty);
    }


    public void ChangeCurrentGamePhase()
    {

        ChangeGamePhase?.Invoke(this, EventArgs.Empty);
    }


    public void ChangeCurrentGameRound()
    {
        CurrentGameRound++;
        ChangeGameRound?.Invoke(this, EventArgs.Empty);
    }


    public void ChangeCurrentPlayerHand(PlayerType faction, int amount)
    {
        ChangeHand?.Invoke(this, new HandEventArgs(faction, amount));
    }


    public void ChangeCurrentPlayerAction(PlayerType faction, int amount)
    {
        ChangeAction?.Invoke(this, new ActionEventArgs(faction, amount));
    }



    public bool MakeMove(Move move)
    {

        if (move.Execute())
        {
            return true;
        }

        return false;

    }


    public bool MakeAttack(Attack attack)
    {

        if (attack.Execute())
        {
            return true;
        }

        return false;

    }


    public bool PlayCard(Card card)
    {

        if (card.Play())
        {
            return true;
        }

        return false;

    }


    public bool DiscardCard(Card card)
    {

        if (card.Discard())
        {
            return true;
        }

        return false;

    }

}





