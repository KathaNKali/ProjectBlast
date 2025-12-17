using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// An Action that shoots using the currently equipped weapon. If your weapon is in auto mode, will shoot until you exit the state, and will only shoot once in SemiAuto mode. You can optionnally have the character face (left/right) the target, and aim at it (if the weapon has a WeaponAim component).
	/// 
	/// EVENT-DRIVEN ARCHITECTURE (TDE Pattern):
	/// - Implements MMEventListener<CombatAllocationEvent> for decoupled communication
	/// - Triggers allocation request events instead of direct CombatCoordinator calls
	/// - Listens for Grant/Deny responses to control firing behavior
	/// - Follows TDE's event-driven pattern for AI actions
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Shoot 3D")]
	//[RequireComponent(typeof(CharacterOrientation3D))]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionShoot3D : AIAction, MMEventListener<CombatAllocationEvent>
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
	[Tooltip("if true, uses cooperative allocation system to coordinate fire with other heroes")]
	public bool EnableSmartFiring = true;
	
	protected CharacterOrientation3D _orientation3D;
	protected Character _character;
	protected WeaponAim _weaponAim;
	protected ProjectileWeapon _projectileWeapon;
	protected Vector3 _weaponAimDirection;
	protected int _numberOfShoots = 0;
	protected bool _shooting = false;
	protected Weapon _targetWeapon;
	
	// Cooperative allocation system tracking
	protected float _weaponDamagePerShot = 0f;
	protected GameObject _currentAllocatedTarget = null;  // Current target with bullet allocation
	protected int _bulletsAllocated = 0;                   // How many bullets allocated to current target
	protected bool _hasAllocation = false;                 // Whether we have active allocation
	protected bool _pendingAllocationRequest = false;      // Event-driven: waiting for Grant/Deny response
	
	// Cached weapon damage (calculate once on weapon change, not every allocation)
	private float _cachedWeaponDamage = 10f;
	private Weapon _cachedDamageWeapon = null;
	
	// Cached reflection components for performance (avoids repeated Type.GetType() and GetProperty() calls)
	private Component _cachedHeroComponent;
	private System.Reflection.PropertyInfo _cachedUnlimitedAmmoProp;
	private System.Reflection.PropertyInfo _cachedCurrentAmmoProp;		/// <summary>
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
			
			// Start listening for allocation events (TDE event-driven pattern)
			this.MMEventStartListening<CombatAllocationEvent>();
			
			// Cache Hero component and properties ONCE (TDE performance pattern)
			// This eliminates 100-1000x slower reflection calls every frame
			var heroType = System.Type.GetType("ProjectBlast.Heroes.Hero, Assembly-CSharp");
			if (heroType != null)
			{
				_cachedHeroComponent = GetComponentInParent(heroType);
				if (_cachedHeroComponent != null)
				{
					_cachedUnlimitedAmmoProp = heroType.GetProperty("UnlimitedAmmo");
					_cachedCurrentAmmoProp = heroType.GetProperty("CurrentAmmo");
				}
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
		
		// Check if we need to request allocation for new target
		if (EnableSmartFiring && _brain.Target != null)
		{
			CheckAndRequestAllocation();
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
	/// Checks if we need to request allocation for new/changed target
	/// </summary>
	protected virtual void CheckAndRequestAllocation()
	{
		if (_brain.Target == null || !CombatCoordinator.HasInstance)
		{
			return;
		}
		
		GameObject targetEnemy = _brain.Target.gameObject;
		
		// If target changed, release old allocation
		if (_currentAllocatedTarget != null && _currentAllocatedTarget != targetEnemy)
		{
			ReleaseCurrentAllocation();
		}
		
		// If we don't have allocation for current target, request it
		if (!_hasAllocation || _currentAllocatedTarget != targetEnemy)
		{
			RequestAllocationForTarget(targetEnemy);
		}
	}
	
	/// <summary>
	/// Requests bullet allocation from coordinator for target (EVENT-DRIVEN)
	/// </summary>
	protected virtual void RequestAllocationForTarget(GameObject target)
	{
		if (target == null || _character == null)
		{
			return;
		}
		
		// Get cached weapon damage (only recalculates if weapon changed)
		_weaponDamagePerShot = GetWeaponDamage();
		
		// Get enemy's effective HP (still use direct call for readonly queries)
		float effectiveHP = CombatCoordinator.HasInstance ? 
			CombatCoordinator.Instance.GetEnemyEffectiveHP(target) : target.GetComponent<Health>()?.CurrentHealth ?? 0f;
		
		// Calculate bullets we need to contribute
		int bulletsNeeded = Mathf.CeilToInt(effectiveHP / _weaponDamagePerShot);
		
		// Get hero's available ammo (check Hero component for ammo limit)
		int maxBulletsAvailable = GetHeroAvailableAmmo();
		
		// Request allocation (min of what's needed vs what we have)
		int bulletsToRequest = Mathf.Min(bulletsNeeded, maxBulletsAvailable);
		
		if (bulletsToRequest <= 0)
		{
			_hasAllocation = false;
			return;
		}
		
		// Get component references for event
		var health = target.GetComponent<Health>();
		if (health == null)
		{
			_hasAllocation = false;
			return;
		}
		
		// EVENT-DRIVEN: Trigger allocation request event (TDE pattern)
		// Response will come via OnMMEvent → HandleAllocationGranted/Denied
		_pendingAllocationRequest = true;
		CombatAllocationEvent.TriggerRequest(
			_character,
			health,
			_character.gameObject,
			target,
			bulletsToRequest,
			_weaponDamagePerShot
		);
	}
	
	/// <summary>
	/// Gets hero's available ammo count
	/// </summary>
	protected virtual int GetHeroAvailableAmmo()
	{
		// Use cached reflection components (100-1000x faster than repeated Type.GetType() calls)
		if (_cachedHeroComponent != null && 
		    _cachedUnlimitedAmmoProp != null && 
		    _cachedCurrentAmmoProp != null)
		{
			try
			{
				bool unlimitedAmmo = (bool)_cachedUnlimitedAmmoProp.GetValue(_cachedHeroComponent);
				if (unlimitedAmmo)
				{
					return 999; // Large number for unlimited
				}
				return (int)_cachedCurrentAmmoProp.GetValue(_cachedHeroComponent);
			}
			catch (System.Exception e)
			{
				Debug.LogWarning($"[AIActionShoot3D] Reflection error getting hero ammo: {e.Message}");
			}
		}
		
		// Fallback: Check weapon's magazine ammo if Hero component not available
		if (TargetHandleWeaponAbility?.CurrentWeapon != null)
		{
			return TargetHandleWeaponAbility.CurrentWeapon.MagazineBased ? 
				TargetHandleWeaponAbility.CurrentWeapon.CurrentAmmoLoaded :
				999;
		}
		
		return 999; // Default to high value if can't determine
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
		
		// Check if we have permission to fire next bullet (cooperative allocation)
		if (EnableSmartFiring && _brain.Target != null)
		{
			// EVENT-DRIVEN: Wait for allocation grant before shooting
			if (_pendingAllocationRequest)
			{
				// Still waiting for Grant/Deny response, don't shoot yet
				return;
			}
			
			if (!_hasAllocation)
			{
				// No allocation, stop shooting
				TargetHandleWeaponAbility.ShootStop();
				_numberOfShoots = 0;
				return;
			}
			
			// Check with coordinator if we can fire next bullet (uses direct call for fast queries)
			if (CombatCoordinator.HasInstance && 
			    !CombatCoordinator.Instance.CanHeroFireNextBullet(_character.gameObject, _brain.Target.gameObject))
			{
				// No more bullets allocated or allocation complete
				TargetHandleWeaponAbility.ShootStop();
				_numberOfShoots = 0;
				
				// Release allocation and find new target
				ReleaseCurrentAllocation();
				ClearCurrentTarget();
				return;
			}
		}
		
		// Fire the weapon
		if (_numberOfShoots < 1)
		{
			_targetWeapon = TargetHandleWeaponAbility.CurrentWeapon;
			TargetHandleWeaponAbility.ShootStart();
			_numberOfShoots++;
		}
		
		// NOTE: Ammo consumption handled by Weapon.ShootRequest() (Option B implementation)
		// No need to call OnHeroFiredBullet() here - would cause double consumption

		// Handle weapon changes
		if ((_targetWeapon == null) || (TargetHandleWeaponAbility.CurrentWeapon != _targetWeapon))
		{
			OnEnterState();
		}
	}
	
	/// <summary>
	/// Gets weapon damage, using cached value if weapon hasn't changed
	/// </summary>
	protected virtual float GetWeaponDamage()
	{
		// Return cached damage if weapon hasn't changed
		if (TargetHandleWeaponAbility?.CurrentWeapon == _cachedDamageWeapon && _cachedWeaponDamage > 0)
		{
			return _cachedWeaponDamage;
		}
		
		// Recalculate if weapon changed
		_cachedDamageWeapon = TargetHandleWeaponAbility?.CurrentWeapon;
		_cachedWeaponDamage = CalculateWeaponDamage();
		return _cachedWeaponDamage;
	}
	
	/// <summary>
	/// Calculates the damage per shot of the current weapon.
	/// PRIORITY: WeaponDataSO (ProjectBlast) > DamageOnTouch (TDE) > Fallback
	/// </summary>
	protected virtual float CalculateWeaponDamage()
	{
		var weapon = TargetHandleWeaponAbility?.CurrentWeapon;
		if (weapon == null)
		{
			return 10f; // Fallback default
		}
		
		// PRIORITY 1: Use ProjectBlast WeaponDataSO system (most consistent and configurable)
		// Use reflection to avoid compile-time dependency on ProjectBlast namespace
		var weaponDataHolderType = System.Type.GetType("ProjectBlast.Data.WeaponDataHolder, Assembly-CSharp");
		if (weaponDataHolderType != null)
		{
			var weaponDataHolder = weapon.GetComponent(weaponDataHolderType);
			if (weaponDataHolder != null)
			{
				var weaponDataProp = weaponDataHolderType.GetProperty("WeaponData");
				if (weaponDataProp != null)
				{
					var weaponData = weaponDataProp.GetValue(weaponDataHolder);
					if (weaponData != null)
					{
						var damagePerShotProp = weaponData.GetType().GetProperty("DamagePerShot");
						if (damagePerShotProp != null)
						{
							return (float)damagePerShotProp.GetValue(weaponData);
						}
					}
				}
			}
		}
		
		// PRIORITY 2: Use TDE ProjectileWeapon's DamageOnTouch
		var projectileWeapon = weapon as ProjectileWeapon;
		if (projectileWeapon?.ObjectPooler != null)
		{
			var simplePooler = projectileWeapon.ObjectPooler as MMSimpleObjectPooler;
			if (simplePooler?.GameObjectToPool != null)
			{
				var damageOnTouch = simplePooler.GameObjectToPool.GetComponent<DamageOnTouch>();
				if (damageOnTouch != null)
				{
					return damageOnTouch.MinDamageCaused;
				}
			}
		}
		
		// PRIORITY 3: Fallback
		return 10f;
	}
	
	/// <summary>
	/// Releases current allocation (EVENT-DRIVEN)
	/// </summary>
	protected virtual void ReleaseCurrentAllocation()
	{
		if (_currentAllocatedTarget != null && _character != null)
		{
			var health = _currentAllocatedTarget.GetComponent<Health>();
			if (health != null)
			{
				// EVENT-DRIVEN: Trigger release event (TDE pattern)
				CombatAllocationEvent.TriggerRelease(
					_character,
					health,
					_character.gameObject,
					_currentAllocatedTarget
				);
			}
		}
		
		_currentAllocatedTarget = null;
		_bulletsAllocated = 0;
		_hasAllocation = false;
		_pendingAllocationRequest = false;
	}
	
	/// <summary>
	/// Clears the current target and releases allocation.
	/// Forces AI to re-detect and find a new target.
	/// </summary>
	protected virtual void ClearCurrentTarget()
	{
		// Release allocation
		ReleaseCurrentAllocation();
		
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
			
			// Cache weapon damage when weapon changes (TDE performance pattern)
			if (TargetHandleWeaponAbility.CurrentWeapon != _cachedDamageWeapon)
			{
				_cachedDamageWeapon = TargetHandleWeaponAbility.CurrentWeapon;
				_cachedWeaponDamage = CalculateWeaponDamage();
			}
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
		
		// Release allocation when exiting shooting state
		if (EnableSmartFiring)
		{
			ReleaseCurrentAllocation();
		}
	}
	
	/// <summary>
	/// Stop listening to events on destroy (TDE pattern)
	/// </summary>
	protected virtual void OnDestroy()
	{
		this.MMEventStopListening<CombatAllocationEvent>();
	}
	
	#region Event Handlers (TDE Pattern)
	
	/// <summary>
	/// TDE event handler - processes allocation responses
	/// </summary>
	public virtual void OnMMEvent(CombatAllocationEvent allocationEvent)
	{
		// Only respond to events involving this character
		if (allocationEvent.Hero != _character)
		{
			return;
		}
		
		switch (allocationEvent.Type)
		{
			case CombatAllocationEvent.EventType.Grant:
				HandleAllocationGranted(allocationEvent);
				break;
				
			case CombatAllocationEvent.EventType.Deny:
				HandleAllocationDenied(allocationEvent);
				break;
		}
	}
	
	/// <summary>
	/// Handles allocation grant response
	/// </summary>
	protected virtual void HandleAllocationGranted(CombatAllocationEvent evt)
	{
		_currentAllocatedTarget = evt.EnemyObject;
		_bulletsAllocated = evt.BulletsGranted;
		_hasAllocation = true;
		_pendingAllocationRequest = false;
	}
	
	/// <summary>
	/// Handles allocation deny response
	/// </summary>
	protected virtual void HandleAllocationDenied(CombatAllocationEvent evt)
	{
		_hasAllocation = false;
		_pendingAllocationRequest = false;
		
		// Handle different denial reasons
		if (evt.Result == AllocationResult.EnemyAlreadyClaimed || 
		    evt.Result == AllocationResult.EnemyFullyAllocated)
		{
			// Clear target and let AI find a different enemy
			ClearCurrentTarget();
		}
	}
	
	#endregion
	}
}