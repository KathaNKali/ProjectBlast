using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// An Action that shoots using the currently equipped weapon. If your weapon is in auto mode, will shoot until you exit the state, and will only shoot once in SemiAuto mode. You can optionnally have the character face (left/right) the target, and aim at it (if the weapon has a WeaponAim component).
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Shoot 3D")]
	//[RequireComponent(typeof(CharacterOrientation3D))]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionShoot3D : AIAction
	{
		public enum AimOrigins { Transform, SpawnPosition }
        
		[Header("Binding")] 
		/// the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.
		[Tooltip("the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.")]
		public CharacterHandleWeapon TargetHandleWeaponAbility;
        
	[Header("Behaviour")]
	/// if true the Character will aim at the target when shooting
	[Tooltip("if true the Character will aim at the target when shooting")]
	public bool AimAtTarget = true;
	/// the point to consider as the aim origin
	[Tooltip("the point to consider as the aim origin")]
	public AimOrigins AimOrigin = AimOrigins.Transform;
	/// an offset to apply to the aim (useful to aim at the head/torso/etc automatically)
	[Tooltip("an offset to apply to the aim (useful to aim at the head/torso/etc automatically)")]
	public Vector3 ShootOffset;
	/// if this is set to true, vertical aim will be locked to remain horizontal
	[Tooltip("if this is set to true, vertical aim will be locked to remain horizontal")]
	public bool LockVerticalAim = false;
	
	[Header("Aim Verification")]
	/// if true, weapon will only fire when properly aimed at target (prevents wasting bullets)
	[Tooltip("if true, weapon will only fire when properly aimed at target (prevents wasting bullets)")]
	public bool RequireAimLock = true;
	/// the maximum angle (in degrees) between weapon aim and target to allow shooting
	[Tooltip("the maximum angle (in degrees) between weapon aim and target to allow shooting")]
	[Range(0f, 45f)]
	public float AimAngleTolerance = 5f;
	
	[Header("Smart Bullet Management")]
	/// if true, coordinates with other heroes to avoid wasting bullets on already-doomed enemies
	[Tooltip("if true, coordinates with other heroes to avoid wasting bullets on already-doomed enemies")]
	public bool EnableSmartFiring = true;
	
	protected CharacterOrientation3D _orientation3D;
	protected Character _character;
	protected WeaponAim _weaponAim;
	protected ProjectileWeapon _projectileWeapon;
	protected Vector3 _weaponAimDirection;
	protected int _numberOfShoots = 0;
	protected bool _shooting = false;
	protected Weapon _targetWeapon;
	
	// Smart bullet management
	protected float _weaponDamagePerShot = 0f;
	protected GameObject _lastTarget = null;		/// <summary>
		/// On init we grab our CharacterHandleWeapon ability
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = GetComponentInParent<Character>();
			_orientation3D = _character?.FindAbility<CharacterOrientation3D>();
			if (TargetHandleWeaponAbility == null)
			{
				TargetHandleWeaponAbility = _character?.FindAbility<CharacterHandleWeapon>();
			}
		}

	/// <summary>
	/// On PerformAction we face and aim if needed, and we shoot
	/// </summary>
	public override void PerformAction()
	{
		// Safety check: Don't perform action if weapon system isn't ready
		if (TargetHandleWeaponAbility == null || TargetHandleWeaponAbility.CurrentWeapon == null)
		{
			return;
		}
		
		MakeChangesToTheWeapon();
		TestAimAtTarget();
		Shoot();
	}		/// <summary>
		/// Makes changes to the weapon to ensure it works ok with AI scripts
		/// </summary>
		protected virtual void MakeChangesToTheWeapon()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				TargetHandleWeaponAbility.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
			}
		}

		/// <summary>
		/// Sets the current aim if needed
		/// </summary>
		protected virtual void Update()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim != null)
				{
					if (_shooting)
					{
						if (LockVerticalAim)
						{
							_weaponAimDirection.y = 0;
						}

						if (AimAtTarget)
						{
							_weaponAim.SetCurrentAim(_weaponAimDirection);    
						}
					}
				}
			}
		}
        
		/// <summary>
		/// Aims at the target if required
		/// </summary>
		protected virtual void TestAimAtTarget()
		{
			if (!AimAtTarget || (_brain.Target == null))
			{
				return;
			}

			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim == null)
				{
					_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
				}

				if (_weaponAim != null)
				{
					if (_projectileWeapon != null)
					{
						if (AimOrigin == AimOrigins.Transform)
						{
							_weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;   
						}
						else if (AimOrigin == AimOrigins.SpawnPosition)
						{
							_projectileWeapon.DetermineSpawnPosition();
							_weaponAimDirection = _brain.Target.position + ShootOffset - _projectileWeapon.SpawnPosition;    
						}
					}
					else
					{
						_weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;
					}                    
				}                
			}
			
			_shooting = true;
		}

	/// <summary>
	/// Activates the weapon
	/// </summary>
	protected virtual void Shoot()
	{
		// Check if weapon is aimed at target before shooting
		if (RequireAimLock && !IsWeaponAimedAtTarget())
		{
			// Weapon is still aiming, don't shoot yet
			if (_shooting || _numberOfShoots > 0)
			{
				TargetHandleWeaponAbility.ShootStop();
			}
			return;
		}
		
		// Smart bullet management - request permission from coordinator
		if (EnableSmartFiring)
		{
			// Update target tracking if changed
			if (_brain.Target != null && _lastTarget != _brain.Target.gameObject)
			{
				// Release old target
				if (_lastTarget != null && CombatCoordinator.HasInstance)
				{
					CombatCoordinator.Instance.ReleaseTarget(gameObject, _lastTarget);
				}
				
				// Calculate weapon damage for new target
				_weaponDamagePerShot = CalculateWeaponDamage();
				_lastTarget = _brain.Target.gameObject;
			}
			
			// ATOMIC: Request shot permission (check + track in one operation)
			if (_brain.Target != null && CombatCoordinator.HasInstance)
			{
				if (!CombatCoordinator.Instance.RequestShot(gameObject, _brain.Target.gameObject, _weaponDamagePerShot))
				{
					// Permission denied - other heroes have already "doomed" this target
					// KEEP TARGET LOCK - just stop firing and wait for enemy to die
					// When enemy dies, OnCurrentTargetDied() will be called automatically
					TargetHandleWeaponAbility.ShootStop();
					_numberOfShoots = 0;
					return; // Don't fire, but maintain target lock
				}
				// Permission granted - bullet is now tracked, proceed to fire
			}
		}
		
		// Fire the weapon
		if (_numberOfShoots < 1)
		{
			_targetWeapon = TargetHandleWeaponAbility.CurrentWeapon;
			TargetHandleWeaponAbility.ShootStart();
			_numberOfShoots++;
		}

		// Handle weapon changes
		if ((_targetWeapon == null) || (TargetHandleWeaponAbility.CurrentWeapon != _targetWeapon))
		{
			OnEnterState();
		}
	}
	

	
	/// <summary>
	/// Calculates the damage per shot of the current weapon.
	/// </summary>
	protected virtual float CalculateWeaponDamage()
	{
		if (TargetHandleWeaponAbility?.CurrentWeapon == null)
		{
			return 10f; // Default fallback
		}
		
		// Try to get damage from projectile's DamageOnTouch
		var projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon as ProjectileWeapon;
		if (projectileWeapon != null && projectileWeapon.ObjectPooler != null)
		{
			// Get the projectile prefab from the object pooler
			GameObject projectilePrefab = null;
			
			// Try as MMSimpleObjectPooler first
			var simplePooler = projectileWeapon.ObjectPooler as MMSimpleObjectPooler;
			if (simplePooler != null)
			{
				projectilePrefab = simplePooler.GameObjectToPool;
			}
			
			// If we have the prefab, get the DamageOnTouch component
			if (projectilePrefab != null)
			{
				var damageOnTouch = projectilePrefab.GetComponent<DamageOnTouch>();
				if (damageOnTouch != null)
				{
					// Use MinDamageCaused (assumes consistent damage)
					return damageOnTouch.MinDamageCaused;
				}
			}
		}
		
		// Fallback to default
		return 10f;
	}
	
	/// <summary>
	/// Clears the current target and releases tracking.
	/// Forces AI to re-detect and find a new target.
	/// </summary>
	protected virtual void ClearCurrentTarget()
	{
		// Release tracking on current target via coordinator
		if (_lastTarget != null && CombatCoordinator.HasInstance)
		{
			CombatCoordinator.Instance.ReleaseTarget(gameObject, _lastTarget);
		}
		
		// Clear tracking
		_lastTarget = null;
		
		// Clear brain's target (forces re-detection)
		if (_brain != null)
		{
			_brain.Target = null;
		}
	}
	
	/// <summary>
	/// Called by EnemyCombatTracker when current target dies.
	/// Immediately clears target so hero can find a new one.
	/// </summary>
	public virtual void OnCurrentTargetDied()
	{
		ClearCurrentTarget();
		_numberOfShoots = 0; // Reset so we can shoot at new target
	}
	
	/// <summary>
	/// Checks if the weapon is properly aimed at the target within the acceptable angle tolerance
	/// </summary>
	/// <returns>True if weapon is aimed at target within tolerance, false otherwise</returns>
	protected virtual bool IsWeaponAimedAtTarget()
	{
		// No target means no aim check needed
		if (_brain.Target == null)
		{
			return false;
		}
		
		// No weapon means can't shoot anyway
		if (TargetHandleWeaponAbility?.CurrentWeapon == null)
		{
			return false;
		}
		
		// Get weapon transform (use spawn position for projectile weapons for accuracy)
		Transform weaponTransform = TargetHandleWeaponAbility.CurrentWeapon.transform;
		Vector3 aimOriginPosition = weaponTransform.position;
		
		// Use projectile spawn position if available for more accurate aim check
		if (_projectileWeapon != null && AimOrigin == AimOrigins.SpawnPosition)
		{
			_projectileWeapon.DetermineSpawnPosition();
			aimOriginPosition = _projectileWeapon.SpawnPosition;
		}
		
		// Calculate direction to target
		Vector3 directionToTarget = (_brain.Target.position + ShootOffset - aimOriginPosition).normalized;
		
		// Get weapon's current forward direction
		Vector3 weaponForward = weaponTransform.forward;
		
		// Lock vertical aim if required (project to horizontal plane)
		if (LockVerticalAim)
		{
			directionToTarget.y = 0;
			directionToTarget.Normalize();
			weaponForward.y = 0;
			weaponForward.Normalize();
		}
		
		// Calculate angle between weapon aim and target direction
		float angleToTarget = Vector3.Angle(weaponForward, directionToTarget);
		
		// Check if within acceptable tolerance
		return angleToTarget <= AimAngleTolerance;
	}	/// <summary>
	/// When entering the state we reset our shoot counter and grab our weapon
	/// </summary>
	public override void OnEnterState()
	{
		base.OnEnterState();
		_numberOfShoots = 0;
		
		// Safety check: Only access CurrentWeapon if it exists
		if (TargetHandleWeaponAbility != null && TargetHandleWeaponAbility.CurrentWeapon != null)
		{
			_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			_projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();
		}
		else
		{
			Debug.LogWarning($"[AIActionShoot3D] OnEnterState called but CurrentWeapon is null on {gameObject.name}. Weapon may not be equipped yet.");
		}
	}		
	
	/// <summary>
	/// When exiting the state we make sure we're not shooting anymore
	/// </summary>
	public override void OnExitState()
	{
		base.OnExitState();
		if (TargetHandleWeaponAbility != null)
		{
			TargetHandleWeaponAbility.ForceStop();    
		}
		_shooting = false;
		
		// Release target tracking when exiting shooting state
		if (EnableSmartFiring && _lastTarget != null && CombatCoordinator.HasInstance)
		{
			CombatCoordinator.Instance.ReleaseTarget(gameObject, _lastTarget);
			_lastTarget = null;
		}
	}
	}
}