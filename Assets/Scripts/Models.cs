using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Models 
{
    #region - Player -

    public enum PlayerStanceEnum
    {
        STAND,
        CROUCH,
        PRONE
    }
     
    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float CameraXSensitivity;
        public float CameraYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Settings")]
        public bool HoldSprint;
        public float MovementSmoothing;

        [Header("Movement - Running")]
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement - Walking")]
        public float WalkingForwardSpeed;
        public float WalkingBackwardSpeed;
        public float WalkingStrafeSpeed;

        [Header("Jumping")]
        public float JumpHeight;
        public float JumpFalloff;
        public float FallingSmoothing;

        [Header("Speed Affectors")]
        public float SpeedAffector = 1;
        public float CrouchSpeedAffector;
        public float ProneSpeedAffector;
        public float FallingSpeedAffector;

        [Header("Is Grounded/Falling")]
        public float IsGroundedRadius;
        public float IsFallingSpeed;
    }

    [Serializable]
    public class PlayerStance
    {
        public float CameraHeight;
        public CapsuleCollider StanceCollider;
    }

    #endregion

    #region - Weapons - 

    public enum WEAPONFIRETYPE
    {
        SEMIAUTO,
        FULLAUTO
    }

    public enum WEAPONTYPE
    {
        PORTAL_GUN,
        MACHINE_GUN
    }
    

    [Serializable]
    public class WeaponSettingsModel
    {
        [Header("Projectile")]
        public float ProjecticleSpeed;

        [Header("Weapon Sway")]
        public float SwayAmount;
        public bool SwayYInverted;
        public bool SwayXInverted;
        public float SwaySmoothing;
        public float SwayResetSmoothing;
        public float SwayClampX;
        public float SwayClampY;

        [Header("Weapon Movement Sway")]
        public float MovementSwayX;
        public float MovementSwayY;
        public bool MovementSwayYInverted;
        public bool MovementSwayXInverted;
        public float MovementSwaySmoothing;
    }

    #endregion
}
