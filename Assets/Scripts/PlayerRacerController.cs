using UnityEngine;

public class PlayerRacerController : RacerController
{
    // Reference to the MoneyManager for UI updates and player-specific functionality
    private MoneyManager moneyManager;
    
    protected override void Start()
    {
        // Get the MoneyManager component
        moneyManager = GetComponent<MoneyManager>();
        if (moneyManager == null)
        {
            Debug.LogError("PlayerRacerController requires a MoneyManager component!");
        }
        
        base.Start();
    }
    
    public override void OnPositionChanged(int newPosition)
    {
        base.OnPositionChanged(newPosition);
        
        // Update the position text in the UI via MoneyManager
        if (moneyManager != null)
        {
            moneyManager.SetPosition(newPosition);
        }
    }
    
    public override void OnLapCompleted()
    {
        base.OnLapCompleted();
        
        // Update the round/lap text in the UI via MoneyManager
        if (moneyManager != null)
        {
           // moneyManager.UpdateRoundText();
        }
    }
}