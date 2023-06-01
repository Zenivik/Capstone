using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    #region Singleton
    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public GameObject player;
    public int Health = 100;
    public float RegenHealthCountdown = 5;
    public TMP_Text text;

    private void Update()
    {
        text.text = "Health: " + Health.ToString();
        if(Health <= 0)
        {
            //Debug.Log("dead");
            Health = 0;
            Destroy(player);
            GameManager.Instance.IsGameOver = true;
        }
        else
        {
            RegenHealthCountdown -= 1 * Time.deltaTime;
        }

    }

    private void FixedUpdate()
    {
        if(Health < 100 && RegenHealthCountdown <= 0)
        {
            Health += 1;
        }
    }

   
}
