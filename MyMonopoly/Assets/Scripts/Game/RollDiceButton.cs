using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollDiceButton : MonoBehaviour
{
    public void OnClick()
    {
        GameManager.instance.LaunchDice();
    }
}
