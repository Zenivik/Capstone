using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private PortalableObject portalableObject;

    public float health = 50f;
    public float AttackRange;
    public bool DoNothing;

    public Animator EnemyAnimator;
    public AudioSource DefaultSound;
    public GameObject RightHand;
    public Transform FeetTransform;
    public LayerMask GroundLayer;

    Transform target;
    NavMeshAgent agent;
    Vector3 PrevPosition;

    float CurrentSpeed;
    int DeathCount = 0;
    float TimeToPlaySound = 3;

    bool IsGrounded;
    bool IsDead = false;

    // Start is called before the first frame update
    void Start()
    {
        portalableObject = GetComponent<PortalableObject>();
        portalableObject.HasTeleported += PortalableObjectOnHasTeleported;

        target = PlayerManager.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        RightHand.GetComponent<BoxCollider>().enabled = false;
        CheckIsGrounded();

        if (!DoNothing)
        {
            if (IsGrounded && !IsDead)
            {
                if (TimeToPlaySound <= 0)
                {
                    DefaultSound.Play();
                    TimeToPlaySound = Random.Range(5f, 11f);
                }
                else TimeToPlaySound -= Time.deltaTime;

                if (!agent.enabled) agent.enabled = true;
                MoveToTarget();
            }
        }

    }

    private void CheckIsGrounded()
    {
        IsGrounded = Physics.CheckSphere(FeetTransform.position, 0.1f, GroundLayer);
    }
    
    private void MoveToTarget()
    {
        if (target != null)
        {
            Vector3 CurrentMove = transform.position - PrevPosition;
            CurrentSpeed = CurrentMove.magnitude / Time.deltaTime;
            PrevPosition = transform.position;
            EnemyAnimator.SetFloat("Speed", CurrentSpeed);

            agent.SetDestination(target.position);

            float distance = Vector3.Distance(target.position, transform.position);
            if (distance <= AttackRange)
            {
                RightHand.GetComponent<BoxCollider>().enabled = true;

            }
            EnemyAnimator.SetFloat("AttackRange", distance);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        //Debug.Log("Health " + health.ToString());

        if (health <= 0f && !IsDead) Die();
    }

    private void Die()
    {
        CurrentSpeed = 0;
        EnemyAnimator.SetFloat("Speed", CurrentSpeed);
        agent.SetDestination(transform.position);

        IsDead = true;
        EnemyAnimator.SetBool("IsDead", IsDead);

        Destroy(gameObject, 3.7f);

        DefaultSound.Stop();

        if (DeathCount <= 0)
        {
            GameManager.Instance.TotalEnemiesKilled++;
            Debug.Log("Killed " + GameManager.Instance.TotalEnemiesKilled.ToString());
            DeathCount++;
        }
    }

    public void Destroy()
    {
        if(DeathCount <= 0)
        {
            GameManager.Instance.TotalEnemiesKilled++;
            DeathCount++;
        }

        DefaultSound.Stop();
        Destroy(gameObject);
    }

    private void PortalableObjectOnHasTeleported(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation)
    {
        // For character controller to update
        Physics.SyncTransforms();
    }

    private void OnDestroy()
    {
        if(portalableObject != null) portalableObject.HasTeleported -= PortalableObjectOnHasTeleported;  
    }

}
