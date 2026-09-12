using UnityEngine;

namespace Character
{
    public class CharacterGroundCheckConfig
    {
        public Transform GroundCheck { get; }
        public float GroundCheckRadius { get; }
        public LayerMask GroundLayer { get; }

        public CharacterGroundCheckConfig(Transform groundCheck, float groundCheckRadius, LayerMask groundLayer)
        {
            GroundCheck = groundCheck;
            GroundCheckRadius = groundCheckRadius;
            GroundLayer = groundLayer;
        }
    }

    public class CharacterMovementConfig
    {
        public Rigidbody2D Rigidbody { get; }
        public CharacterStats Stats { get; }

        public CharacterMovementConfig(Rigidbody2D rigidbody, CharacterStats stats)
        {
            Rigidbody = rigidbody;
            Stats = stats;
        }
    }

    public class CharacterCombatConfig
    {
        public Weapon CurrentWeapon { get; }
        public WeaponController WeaponController { get; }

        public CharacterCombatConfig(Weapon currentWeapon, WeaponController weaponController)
        {
            CurrentWeapon = currentWeapon;
            WeaponController = weaponController;
        }
    }
}
