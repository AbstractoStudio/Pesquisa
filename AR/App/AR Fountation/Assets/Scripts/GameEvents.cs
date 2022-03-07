using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    void Awake() => current = this;

    public event Action onEnteredPig;
    public event Action onExitedPig;
    public event Action onEnteredMinotaur;
    public event Action onExitedMinotaur;

    public void EnteredPig() {
        if (onEnteredPig != null) onEnteredPig();
    }
    public void ExitedPig(){
        if (onExitedPig != null) onExitedPig();
    }
    public void EnteredMinotaur(){
        if (onEnteredMinotaur != null) onEnteredMinotaur();
    }
    public void ExitedMinotaur(){
        if (onExitedMinotaur != null) onExitedMinotaur();
    }

}
