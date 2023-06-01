using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deathbox : MonoBehaviour
{
    [SerializeField] float damage;

    private void OnTriggerEnter(Collider other)
    {
        string tag = other.tag;

        if (tag.Equals("Enemy"))
        {
            other.GetComponent<EnemyController>().TakeDamage(damage);
        }
        else if (tag.Equals("Player"))
        {
            PlayerManager.instance.Health -= (int)damage;
        }
    }
}
