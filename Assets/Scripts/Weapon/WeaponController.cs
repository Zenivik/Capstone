using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private PlayerController PlayerController;

    public LayerMask layerMask;

    [Header("References")]
    [SerializeField]
    private Camera PlayerCamera;
    public Animator WeaponAnimator;
    public Transform ProjectileSpawn;
    public GameObject ProjectilePrefab;
    public GameObject GunshotSoundPrefab;
    public GameObject ProjectileImpact;
    public GameObject PortalPreview;
    public GameObject PortalPrefab;
    public ParticleSystem MuzzleFlash;

    [Header("Shooting")]
    public WEAPONTYPE WeaponType;
    public float Damage;
    public float RateOfFire;
    private float TimeToFire = 0f;
    public List<WEAPONFIRETYPE> AllowedFireTypes;
    public WEAPONFIRETYPE CurrentFireType;
    [HideInInspector]
    public bool IsShooting;


    [Header("Settings")]
    public WeaponSettingsModel Settings;

    [HideInInspector]
    public bool IsInitialized;

    #region Rotations and Velocity

    Vector3 NewWeaponRotation;
    Vector3 NewWeaponRotationVelocity;

    Vector3 TargetWeaponRotation;
    Vector3 TargetWeaponRotationVelocity;

    Vector3 NewWeaponMovementRotation;
    Vector3 NewWeaponMovementRotationVelocity;

    Vector3 TargetWeaponMovementRotation;
    Vector3 TargetWeaponMovementRotationVelocity;

    #endregion

    private bool IsGroundedTrigger;
    private float FallingDelay;

    private bool IsPortalPreviewActive;
    private GameObject PortPreview;


    [HideInInspector]
    public bool FireLeftPortal;
    [HideInInspector]
    public bool FireRightPortal;

    Ray ray;
    RaycastHit hitInfo;


    private void Start()
    {
        NewWeaponRotation = transform.localRotation.eulerAngles;
        CurrentFireType = AllowedFireTypes.First();
    }
    public void Initialize(PlayerController playerController)
    {
        PlayerController = playerController;
        IsInitialized = true;
        IsPortalPreviewActive = false;
    }

    private void Update()
    {
        if (!IsInitialized) return;

        PreviewPortal();
        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateShooting();
    }

    private void OnDisable()
    {
        IsPortalPreviewActive = false;
        Destroy(PortPreview);
    }

    #region Helper Methods

    private RaycastHit GetRaycast()
    {
        ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);

        if(Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
        {
            return hitInfo;
        }

        return hitInfo;
    }

    private RaycastHit GetRaycast(LayerMask layerMask)
    {
        ray = new Ray(ProjectileSpawn.position, ProjectileSpawn.transform.forward);

        if (Physics.Raycast(ray, out hitInfo, 100, layerMask))
        {
            return hitInfo;
        }

        return hitInfo;
    }

    #endregion

    #region - Shooting -

    private void CalculateShooting()
    {
        if (IsShooting)
        {
            if(Time.time >= TimeToFire)
            {
                TimeToFire = Time.time + 1f / RateOfFire;
                Shoot();
                if(CurrentFireType == WEAPONFIRETYPE.SEMIAUTO)
                {
                    IsShooting = false;
                }
            }
        }
    }

    private void Shoot()
    {
        if(WeaponType == WEAPONTYPE.PORTAL_GUN)
        {
            var projectile = Instantiate(ProjectilePrefab, ProjectileSpawn.position, Quaternion.identity);
            projectile.GetComponent<PortalProjectile>().PortalGun = this;

            var gunshot = Instantiate(GunshotSoundPrefab, ProjectileSpawn.position, Quaternion.identity);
            Destroy(gunshot, 0.75f);

            Vector3 targetPosition = hitInfo.point;
        
            projectile.GetComponent<Rigidbody>().velocity = (targetPosition - ProjectileSpawn.position).normalized * Settings.ProjecticleSpeed;

            CreatePortal();
        }

        if(WeaponType == WEAPONTYPE.MACHINE_GUN)
        {
            MuzzleFlash.Play();
            var projectile = Instantiate(ProjectilePrefab, ProjectileSpawn.position, Quaternion.identity);
            Destroy(projectile, 0.75f);

            hitInfo = GetRaycast();

            if (Portal.RaycastRecursive(PlayerCamera.transform.position, PlayerCamera.transform.forward, layerMask.value, 20, out hitInfo))
            {
                if (hitInfo.collider != null)
                {
                    if (hitInfo.collider.tag.Equals("Enemy"))
                    {
                        // Enemy takes damage
                        hitInfo.transform.GetComponent<EnemyController>().TakeDamage(Damage);

                        // Impact effect
                        var impact = Instantiate(ProjectileImpact, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        Destroy(impact, 1f);
                    }
                }
            }
        }
    }
    #endregion

    #region Portal Methods

    private void CreatePortal()
    {
        if (FireLeftPortal)
        {
            if (GameManager.Instance.LeftPortal != null)
            {
                Destroy(GameManager.Instance.LeftPortal);
                GameManager.Instance.RightPortal.GetComponent<Portal>().TargetPortal = null;
                GameManager.Instance.RightPortal.GetComponent<Portal>().VisiblePortals[0] = null;
            }

            GameManager.Instance.LeftPortal = Instantiate(PortalPrefab, GameManager.Instance.LeftPortalTransform.position,
            GameManager.Instance.LeftPortalTransform.rotation);

             
            // Set Left's Target portal if Right portal exists. Set visible portal(s) for recursive rendering
            if (GameManager.Instance.RightPortal != null)
            {
                GameManager.Instance.LeftPortal.GetComponent<Portal>().TargetPortal = GameManager.Instance.RightPortal.GetComponent<Portal>();

                GameManager.Instance.LeftPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.RightPortal.GetComponent<Portal>();
                GameManager.Instance.RightPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.LeftPortal.GetComponent<Portal>();
            }

            // set Right's Target portal if Left Portal came after Right Portal. Set visible portal(s) for recursive rendering
            if (GameManager.Instance.RightPortal != null && GameManager.Instance.RightPortal.GetComponent<Portal>().TargetPortal == null)
            {
                GameManager.Instance.RightPortal.GetComponent<Portal>().TargetPortal = GameManager.Instance.LeftPortal.GetComponent<Portal>();

                GameManager.Instance.LeftPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.RightPortal.GetComponent<Portal>();
                GameManager.Instance.RightPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.LeftPortal.GetComponent<Portal>();
            }

        }
        else if (FireRightPortal)
        {
            if (GameManager.Instance.RightPortal != null)
            {
                Destroy(GameManager.Instance.RightPortal);
                GameManager.Instance.LeftPortal.GetComponent<Portal>().TargetPortal = null;
                GameManager.Instance.LeftPortal.GetComponent<Portal>().VisiblePortals[0] = null;
            }

            GameManager.Instance.RightPortal = Instantiate(PortalPrefab, GameManager.Instance.RightPortalTransform.position,
                GameManager.Instance.RightPortalTransform.rotation);

            // Set visible portal(s) for recursive rendering

            // Set Right's Target portal if Left Portal exists. Set visible portal(s) for recursive rendering
            if (GameManager.Instance.LeftPortal != null)
            {
                GameManager.Instance.RightPortal.GetComponent<Portal>().TargetPortal = GameManager.Instance.LeftPortal.GetComponent<Portal>();

                GameManager.Instance.RightPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.LeftPortal.GetComponent<Portal>();
                GameManager.Instance.LeftPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.RightPortal.GetComponent<Portal>();
            }

            // Set Left's Target portal if Right Portal came after Left Portal. Set visible portal(s) for recursive rendering
            if (GameManager.Instance.LeftPortal != null && GameManager.Instance.LeftPortal.GetComponent<Portal>().TargetPortal == null)
            {
                GameManager.Instance.LeftPortal.GetComponent<Portal>().TargetPortal = GameManager.Instance.RightPortal.GetComponent<Portal>();

                GameManager.Instance.RightPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.LeftPortal.GetComponent<Portal>();
                GameManager.Instance.LeftPortal.GetComponent<Portal>().VisiblePortals[0] = GameManager.Instance.RightPortal.GetComponent<Portal>();
            }

        }
    }

    private void PreviewPortal()
    {

        if (WeaponType != WEAPONTYPE.PORTAL_GUN) return;

        hitInfo = GetRaycast();

        if (Portal.RaycastRecursive(PlayerCamera.transform.position, PlayerCamera.transform.forward, layerMask.value, 20, out hitInfo))
        {
            if (hitInfo.collider != null)
            {
                if (!IsPortalPreviewActive)
                {
                    IsPortalPreviewActive = true;
                    PortPreview = Instantiate(PortalPreview);
                }
                else
                {
                    string tag = hitInfo.transform.gameObject.tag;
                    // For Ground/Ceiling Preview
                    if (tag.Equals("Ground") || tag.Equals("Ceiling")) 
                    {
                        // Reset rotation
                        PortalPreview.transform.rotation = Quaternion.identity;

                        Vector3 positionOffset = hitInfo.point;

                        if (hitInfo.transform.gameObject.tag.Equals("Ground")) positionOffset.y += 0.17f;
                        else positionOffset.y -= 0.17f;

                        PortPreview.transform.position = positionOffset;
                        PortPreview.transform.rotation = Quaternion.LookRotation(PlayerCamera.transform.forward, hitInfo.normal);

                        Vector3 portPreviewRotate = PortPreview.transform.eulerAngles;

                        if (tag.Equals("Ground")) portPreviewRotate.x = 90;
                        else portPreviewRotate.x = -90;
                        PortPreview.transform.rotation = Quaternion.Euler(portPreviewRotate);

                        SetPortalTransform(PortPreview.transform);
                    }
                    else
                    {
                        // Wall Preview
                        // Reset Rotation
                        PortalPreview.transform.rotation = Quaternion.identity;

                        PortPreview.transform.position = hitInfo.point + (hitInfo.normal / 2);

                        Vector3 wallRotation = hitInfo.transform.rotation.eulerAngles;

                        PortPreview.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -hitInfo.normal);
                        Vector3 portPreviewRotate = PortPreview.transform.eulerAngles;
                        if (portPreviewRotate.x <= 0) portPreviewRotate.x = 0;
                        PortPreview.transform.rotation = Quaternion.Euler(portPreviewRotate);

                        Quaternion newRotation = PortPreview.transform.rotation;
                        Vector3 portalRotation = newRotation.eulerAngles;

                        // Make sure it's not upside down
                        if(portalRotation.y == 180)
                        {
                            portalRotation.x = 180;
                            portalRotation.y = 0;
                        }
                        newRotation.eulerAngles = portalRotation;
                        PortPreview.transform.rotation = newRotation;
                        SetPortalTransform(PortPreview.transform);
                    }
                }

            }

        }
        else
        {
            IsPortalPreviewActive = false;
            Destroy(PortPreview);
        }
    }

    private void SetPortalTransform(Transform transform)
    {
        if(FireLeftPortal)
        {
            GameManager.Instance.LeftPortalTransform = transform;
        }
        else if(FireRightPortal)
        {
            GameManager.Instance.RightPortalTransform = transform;
        }
    }

    #endregion

    public void TriggerJump()
    {
        IsGroundedTrigger = false;

        WeaponAnimator.SetTrigger("Jump");
    }

    private void CalculateWeaponRotation()
    {
        

        // Look Left and right
        TargetWeaponRotation.y += Settings.SwayAmount * (Settings.SwayXInverted ? -PlayerController.Input_Camera.x : PlayerController.Input_Camera.x) * Time.deltaTime;
        TargetWeaponRotation.y = Mathf.Clamp(TargetWeaponRotation.y, -Settings.SwayClampY, Settings.SwayClampY);

        //Look Up and Down
        TargetWeaponRotation.x += Settings.SwayAmount * (Settings.SwayYInverted ? PlayerController.Input_Camera.y : -PlayerController.Input_Camera.y) * Time.deltaTime;
        TargetWeaponRotation.x = Mathf.Clamp(TargetWeaponRotation.x, -Settings.SwayClampX, Settings.SwayClampX);

        TargetWeaponRotation.z = TargetWeaponRotation.y;

        TargetWeaponRotation = Vector3.SmoothDamp(TargetWeaponRotation, Vector3.zero,
            ref TargetWeaponRotationVelocity, Settings.SwayResetSmoothing);

        NewWeaponRotation = Vector3.SmoothDamp(NewWeaponRotation, TargetWeaponRotation,
            ref NewWeaponRotationVelocity, Settings.SwaySmoothing);

        TargetWeaponMovementRotation.z = Settings.MovementSwayX * (Settings.MovementSwayXInverted ? -PlayerController.Input_Movement.x : PlayerController.Input_Movement.x);
        TargetWeaponMovementRotation.x = Settings.MovementSwayY * (Settings.MovementSwayYInverted ? -PlayerController.Input_Movement.y : PlayerController.Input_Movement.y);

        TargetWeaponMovementRotation = Vector3.SmoothDamp(TargetWeaponMovementRotation, Vector3.zero,
            ref TargetWeaponMovementRotationVelocity, Settings.MovementSwaySmoothing);

        NewWeaponMovementRotation = Vector3.SmoothDamp(NewWeaponMovementRotation, TargetWeaponMovementRotation,
            ref NewWeaponMovementRotationVelocity, Settings.MovementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(NewWeaponRotation + NewWeaponMovementRotation);
    }

    private void SetWeaponAnimations()
    {

        if (IsGroundedTrigger) FallingDelay = 0;
        else FallingDelay += Time.deltaTime;

        if (PlayerController.IsGrounded && !IsGroundedTrigger && FallingDelay > 0.1f)
        {
            WeaponAnimator.SetTrigger("Land");
            IsGroundedTrigger = true;
        }
        else if (!PlayerController.IsGrounded && IsGroundedTrigger)
        {
            WeaponAnimator.SetTrigger("Falling");
            IsGroundedTrigger = false;
        }

        WeaponAnimator.SetBool("IsSprinting", PlayerController.IsSprinting);
        WeaponAnimator.SetFloat("WeaponAnimationSpeed", PlayerController.WeaponAnimationSpeed);
    }

}
