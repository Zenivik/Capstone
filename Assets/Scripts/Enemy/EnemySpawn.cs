using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject ZombiePrefab;

    [SerializeField]
    private float MinCountDown = 3;
    [SerializeField]
    private float MaxCountDown = 5;

    float countdown = 5;
    bool IsPlayerInRange = false;

    private void Awake()
    {
    }

    private void Start()
    {
        gameObject.SetActive(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.TimeBetweenWaves <= 0 && IsPlayerInRange)
        {
            if(countdown <= 0 && GameManager.Instance.TotalEnemySpawned < GameManager.Instance.MaxEnemySpawned)
            {
                //StartCoroutine(SpawnEnemy());
                SpawnEnemy();
                countdown = Random.Range(MinCountDown, MaxCountDown);
            }
            countdown -= Time.deltaTime;
        }
    }


    private void SpawnEnemy()
    {
        Instantiate(ZombiePrefab, transform.position, Quaternion.identity);
        GameManager.Instance.TotalEnemySpawned++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player")) IsPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player")) IsPlayerInRange = false;
    }
}
