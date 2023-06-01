using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float LifeTime = 1;

    private void Awake()
    {
        //Destroy(gameObject, LifeTime);
    }
}
