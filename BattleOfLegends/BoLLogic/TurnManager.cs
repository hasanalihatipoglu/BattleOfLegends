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
        ChangeTurnPhase?.Invoke(this, EventArgs.Empty);
    }




    public void ChangeCurrentPlayer()
    {
        CurrentPlayer = CurrentPlayer.Opponent();
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





