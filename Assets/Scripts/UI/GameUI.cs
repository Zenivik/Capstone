using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image Hotkey1Background;
    public Image Hotkey2Background;
    public Image Crosshair;

    // Start is called before the first frame update
    void Start()
    {
        Hotkey1Background.enabled = true;
        Hotkey2Background.enabled = false;
        
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) Crosshair.enabled = false;
    }

    public void Hotkey1Pressed()
    {
        if (!Hotkey1Background.enabled)
        {
            Hotkey1Background.enabled = true;
            Hotkey2Background.enabled = false;
        }
    }

    public void Hotkey2Pressed()
    {
        if (!Hotkey2Background.enabled)
        {
            Hotkey2Background.enabled = true;
            Hotkey1Background.enabled = false;
        }
    }
}