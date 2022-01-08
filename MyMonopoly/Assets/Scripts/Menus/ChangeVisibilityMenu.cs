using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeVisibilityMenu : MonoBehaviour
{
    [SerializeField] GameObject menuToHide;
    [SerializeField] GameObject menuToShow;

    public void ChangeVisibility()
    {
        menuToHide.SetActive(false);
        menuToShow.SetActive(true);
    }
}
