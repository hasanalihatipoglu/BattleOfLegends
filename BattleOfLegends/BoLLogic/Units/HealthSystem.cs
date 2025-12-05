namespace BoLLogic;

public class HealthSystem
{

    public event EventHandler OnDead;
    public event EventHandler OnDamaged;

    int health;
    public Unit Unit { get; set; }

    public event EventHandler<StateChangedEventArgs> ChangeUnitState;

    public void Damage(int damageAmount)
    {
        health -= damageAmount;

        if (health < 0) 
        {             
            health = 0;
        }

     //   OnDamaged?.Invoke(this, new DeadEventArgs(Unit));

        if (health == 0) 
        {
            Die();
        }

    }

    public void Die()
    {
        SoundController.Instance.PlaySound("break_glass");

        // Use event system for state change instead of direct assignment
        ChangeUnitState?.Invoke(this, new StateChangedEventArgs(Unit, UnitState.Dead));

        // Update tile state
        if (Unit.Tile != null)
        {
            Unit.Tile.Occupied = false;
            Unit.Tile.Unit = null;
        }

        // Trigger death event
        OnDead?.Invoke(this, EventArgs.Empty);

        // Clean up event subscriptions to prevent memory leaks
        Unit.Dispose();
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int healthAmount)
    {
        health = healthAmount;      
    }
}
