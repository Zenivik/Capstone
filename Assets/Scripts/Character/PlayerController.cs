using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private PortalableObject portalableObject;
    private DefaultInput DefaultInput;

    public GameUI ButtonUI;
    public AudioSource Footstep;

    [HideInInspector]
    public Vector2 Input_Movement;
    [HideInInspector]
    public Vector2 Input_Camera;

    private Vector3 NewCameraRotation;
    private Vector3 NewCharacterRotation;

    [Header("References")]
    public Transform CameraHolder;
    public Transform FeetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float CameraClampYMin = -70;
    public float CameraClampYMax = 80;
    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;

    [Header("Gravity")]
    public float GravityAmount;
    public float GravityMin;
    private float PlayerGravity;

    public Vector3 JumpingForce;
    private Vector3 JumpingForceVelocity;

    [Header("Stance")]
    public PlayerStanceEnum PlayerStance;
    public float PlayerStanceSmoothing;
    public PlayerStance PlayerStandStance;
    public PlayerStance PlayerCrouchStance;
    public PlayerStance PlayerProneStance;

    private float StanceCheckErrorMargin = 0.05f;
    private float CameraHeight;
    private float CameraHeightVelocity;

    private Vector3 StanceColliderSetterVelocity;
    private float StanceColliderHeightVelocity;
    
    [HideInInspector]
    public bool IsSprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public WeaponController CurrentWeaponController;
    public GameObject CurrentWeapon;
    public List<GameObject> Weapons;
    public float WeaponAnimationSpeed;

    [HideInInspector]
    public bool IsGrounded;
    [HideInInspector]
    public bool IsFalling;

    #region - Awake -

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        portalableObject = GetComponent<PortalableObject>();
        portalableObject.HasTeleported += PortalableObjectOnHasTeleported;
        DefaultInput = new DefaultInput();

        DefaultInput.Character.Camera.performed += e => Input_Camera = e.ReadValue<Vector2>();

        DefaultInput.Character.Movement.performed += e => Input_Movement = e.ReadValue<Vector2>();
        DefaultInput.Character.Sprint.performed += e => ToggleSprint();
        DefaultInput.Character.SprintReleased.performed += e => StopSprint();
        DefaultInput.Character.Jump.performed += e => Jump();

        DefaultInput.Character.Crouch.performed += e => Crouch();
        DefaultInput.Character.Prone.performed += e => Prone();

        DefaultInput.Weapon.Fire1Pressed.performed += e => Shooting1Pressed();
        DefaultInput.Weapon.Fire1Released.performed += e => Shooting1Released();

        DefaultInput.Weapon.Fire2Pressed.performed += e => Shooting2Pressed();
        DefaultInput.Weapon.Fire2Released.performed += e => Shooting2Released();

        DefaultInput.Weapon.WeaponHotKey1Pressed.performed += e => WeaponOne();
        DefaultInput.Weapon.WeaponHotKey2Pressed.performed += e => WeaponTwo();

        DefaultInput.Enable();

        NewCameraRotation = CameraHolder.localRotation.eulerAngles;
        NewCharacterRotation = transform.localRotation.eulerAngles;

        CameraHeight = CameraHolder.localPosition.y;

        if (CurrentWeaponController) CurrentWeaponController.Initialize(this);

        if (CurrentWeapon != Weapons[1]) WeaponOne();

    }

    #endregion

    #region - Update -

    private void Update()
    {
        SetIsGounded();
        SetIsFalling();

        CalculateJump();
        CalculateStance();
    }

    private void FixedUpdate()
    {
        CalculateView();
        CalculateMovement();
    }
    #endregion

    #region - Weapon Changing -

    private void WeaponOne()
    {
        if(CurrentWeapon != Weapons[0])
        {
            CurrentWeapon.SetActive(false);

            CurrentWeapon = Weapons[0];
            CurrentWeaponController = CurrentWeapon.GetComponent<WeaponController>();
            CurrentWeapon.SetActive(true);
            ButtonUI.Hotkey1Pressed();
        }
    }

    private void WeaponTwo()
    {
        if(CurrentWeapon != Weapons[1])
        {
            CurrentWeapon.SetActive(false);

            CurrentWeapon = Weapons[1];
            CurrentWeaponController = CurrentWeapon.GetComponent<WeaponController>();

            if (!CurrentWeaponController.IsInitialized) CurrentWeaponController.Initialize(this);

            CurrentWeapon.SetActive(true);
            ButtonUI.Hotkey2Pressed();
        }
    }

    #endregion

    #region - Shooting -

    private void Shooting1Pressed()
    {
        if (CurrentWeaponController) CurrentWeaponController.IsShooting = true;
        if (CurrentWeaponController.WeaponType == WEAPONTYPE.PORTAL_GUN) CurrentWeaponController.FireLeftPortal = true;
    }

    private void Shooting1Released()
    {
        if (CurrentWeaponController) CurrentWeaponController.IsShooting = false;
        if (CurrentWeaponController.WeaponType == WEAPONTYPE.PORTAL_GUN) CurrentWeaponController.FireLeftPortal = false;
    }

    private void Shooting2Pressed()
    {
        if (CurrentWeaponController.WeaponType == WEAPONTYPE.PORTAL_GUN) CurrentWeaponController.IsShooting = true;
        if (CurrentWeaponController.WeaponType == WEAPONTYPE.PORTAL_GUN) CurrentWeaponController.FireRightPortal = true;
    }

    private void Shooting2Released()
    {
        if (CurrentWeaponController.WeaponType == WEAPONTYPE.PORTAL_GUN) CurrentWeaponController.IsShooting = false;
        if (CurrentWeaponController.WeaponType == WEAPONTYPE.PORTAL_GUN) CurrentWeaponController.FireRightPortal = false;
    }

    #endregion

    #region - IsFalling / IsGrounded -

    private void SetIsGounded()
    {
        IsGrounded = Physics.CheckSphere(FeetTransform.position, playerSettings.IsGroundedRadius, GroundLayer);
    }

    private void SetIsFalling()
    {
        IsFalling = (!IsGrounded && characterController.velocity.magnitude > playerSettings.IsFallingSpeed);   
    }

    #endregion

    #region - View/Movement -

    private void CalculateView()
    {
        // Turn player
        transform.Rotate(Vector3.up * Input_Camera.x * playerSettings.CameraXSensitivity * Time.deltaTime);

        //Up and Down
        NewCameraRotation.x += playerSettings.CameraYSensitivity * (playerSettings.ViewYInverted ? Input_Camera.y : -Input_Camera.y) * Time.deltaTime;
        NewCameraRotation.x = Mathf.Clamp(NewCameraRotation.x, CameraClampYMin, CameraClampYMax);
        CameraHolder.localRotation = Quaternion.Euler(NewCameraRotation);
        
    }

    private void CalculateMovement()
    {
        if (Input_Movement.y <= 0.2f) IsSprinting = false;

        var verticalSpeed = playerSettings.WalkingForwardSpeed;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

        if (IsSprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }


        // Affectors
        if (!IsGrounded) playerSettings.SpeedAffector = playerSettings.FallingSpeedAffector;
        else if (PlayerStance == PlayerStanceEnum.CROUCH) playerSettings.SpeedAffector = playerSettings.CrouchSpeedAffector;
        else if (PlayerStance == PlayerStanceEnum.PRONE) playerSettings.SpeedAffector = playerSettings.ProneSpeedAffector;
        else playerSettings.SpeedAffector = 1;

        WeaponAnimationSpeed = characterController.velocity.magnitude / (playerSettings.WalkingForwardSpeed * playerSettings.SpeedAffector);

        if (WeaponAnimationSpeed > 1) WeaponAnimationSpeed = 1;

        verticalSpeed *= playerSettings.SpeedAffector;
        horizontalSpeed *= playerSettings.SpeedAffector;
        //

        var newMovementSpeedZ = verticalSpeed * Input_Movement.y * Time.deltaTime;
        var newMovementSpeedX = horizontalSpeed * Input_Movement.x * Time.deltaTime;

        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(newMovementSpeedX, 0, newMovementSpeedZ),
            ref newMovementSpeedVelocity, IsGrounded ? playerSettings.MovementSmoothing : playerSettings.FallingSmoothing);

        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        if(PlayerGravity > GravityMin)
        {
            PlayerGravity -= GravityAmount * Time.deltaTime;
        }

        if(PlayerGravity < -0.1f && IsGrounded)
        {
            PlayerGravity = -0.1f;
        }

        movementSpeed.y += PlayerGravity;
        movementSpeed += JumpingForce * Time.deltaTime;

        characterController.Move(movementSpeed);    }

    #endregion

    #region - Jump -
    private void CalculateJump()
    {
        JumpingForce = Vector3.SmoothDamp(JumpingForce, Vector3.zero, ref JumpingForceVelocity, playerSettings.JumpFalloff);
    }

    private void Jump() 
    {
        if (!IsGrounded || PlayerStance == PlayerStanceEnum.PRONE) return;

        if (PlayerStance == PlayerStanceEnum.CROUCH)
        {
            if(StanceCheck(PlayerStandStance.StanceCollider.height)) return;
            PlayerStance = PlayerStanceEnum.STAND;
            return;
        }

        // jump
        JumpingForce = Vector3.up * playerSettings.JumpHeight;
        PlayerGravity = 0;
        CurrentWeaponController.TriggerJump();
    }

    #endregion

    #region - Stance -

    private void CalculateStance()
    {
        var currentStance = PlayerStandStance;
        //currentStance.StanceCollider.enabled = true;

        if(PlayerStance == PlayerStanceEnum.CROUCH)
        {
            //currentStance.StanceCollider.enabled = false;
            currentStance = PlayerCrouchStance;
            //currentStance.StanceCollider.enabled = true;
        }
        else if(PlayerStance == PlayerStanceEnum.PRONE)
        {
            //currentStance.StanceCollider.enabled = false;
            currentStance = PlayerProneStance;
            //currentStance.StanceCollider.enabled = true;
        }

        CameraHeight = Mathf.SmoothDamp(CameraHolder.localPosition.y, currentStance.CameraHeight,
            ref CameraHeightVelocity, PlayerStanceSmoothing);

        CameraHolder.localPosition = new Vector3(CameraHolder.localPosition.x, CameraHeight, CameraHolder.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height,
            ref StanceColliderHeightVelocity, PlayerStanceSmoothing);

        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center,
            ref StanceColliderSetterVelocity, PlayerStanceSmoothing);
    }

    private void Crouch()
    {
        if(PlayerStance == PlayerStanceEnum.CROUCH)
        {
            if (StanceCheck(PlayerStandStance.StanceCollider.height)) return;
            PlayerStance = PlayerStanceEnum.STAND;
            return;
        }

        if (StanceCheck(PlayerCrouchStance.StanceCollider.height)) return;
        PlayerStance = PlayerStanceEnum.CROUCH;
    }

    private void Prone()
    {
        PlayerStance = PlayerStanceEnum.PRONE;
    }
    
    private bool StanceCheck(float StanceCheckHeight)
    {
        var start = new Vector3(FeetTransform.position.x,
            FeetTransform.position.y + characterController.radius + StanceCheckErrorMargin, FeetTransform.position.z);

        var end = new Vector3(FeetTransform.position.x,
            FeetTransform.position.y - characterController.radius - StanceCheckErrorMargin + StanceCheckHeight, FeetTransform.position.z);


        return Physics.CheckCapsule(start, end, characterController.radius, PlayerLayer);
    }

    #endregion

    #region - Sprinting - 

    private void ToggleSprint()
    {
        if (Input_Movement.y <= 0.2f)
        {
            IsSprinting = false;
            return;
        }

        IsSprinting = !IsSprinting;
    }
    
    private void StopSprint()
    {
        if (playerSettings.HoldSprint) IsSprinting = false;
    }

    #endregion

    #region - Gizmos -

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(FeetTransform.position, playerSettings.IsGroundedRadius);
    }

    #endregion

    private void PortalableObjectOnHasTeleported(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation)
    {
        // For character controller to update
        Physics.SyncTransforms();
    }

    private void OnDestroy()
    {
        portalableObject.HasTeleported -= PortalableObjectOnHasTeleported;
    }
}
