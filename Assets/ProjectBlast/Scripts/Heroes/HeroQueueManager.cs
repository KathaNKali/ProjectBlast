using UnityEngine;
using MoreMountains.Tools;
using ProjectBlast.Grid;
using System.Collections;
using System.Collections.Generic;

namespace ProjectBlast.Heroes
{
    /// <summary>
    /// Manages the vertical lane-based hero queue system.
    /// Each column is an independent queue where heroes move vertically upward.
    /// 
    /// Flow: Passive (2+ rows) → Active (1 row) → Firing (no lanes)
    /// </summary>
    public class HeroQueueManager : MMSingleton<HeroQueueManager>
    {
        [Header("Hero Prefab")]
        [Tooltip("The hero prefab to spawn for testing")]
        public GameObject TestHeroPrefab;
        
        [Header("Spawn Settings")]
        [Tooltip("Automatically fill Passive and Active grids on Start")]
        public bool AutoSpawnOnStart = true;
        
        [Header("Lane Queue Settings")]
        [Tooltip("Enable vertical lane-based queue system")]
        public bool EnableLaneQueueSystem = true;
        
        [Header("Animation Settings")]
        [Tooltip("Duration of hero movement animation (Lerp)")]
        public float AnimationDuration = 0.3f;
        
        [Tooltip("Delay before starting lane shift animation after deployment")]
        public float AnimationDelay = 0.2f;
        
        [Header("Deployment Settings")]
        [Tooltip("Selected hero waiting to be deployed to Firing zone")]
        public Hero SelectedHero;
        
        [Header("Debug")]
        public bool ShowDebugLogs = true;
        
        private List<Hero> _allHeroes = new List<Hero>();
        private bool _isAnimating = false;
        
        /// <summary>
        /// Whether animations are currently playing (blocks input)
        /// </summary>
        public bool IsAnimating => _isAnimating;
        
        protected override void Awake()
        {
            base.Awake();
        }
        
        void Start()
        {
            if (AutoSpawnOnStart)
            {
                SpawnTestHeroes();
            }
        }
        
        /// <summary>
        /// Spawns test heroes in all available Passive and Active grid slots
        /// </summary>
        public void SpawnTestHeroes()
        {
            if (TestHeroPrefab == null)
            {
                Debug.LogError("[HeroQueueManager] TestHeroPrefab is not assigned!");
                return;
            }
            
            if (GridManager.Instance == null)
            {
                Debug.LogError("[HeroQueueManager] GridManager.Instance not found!");
                return;
            }
            
            // Spawn in Passive zone
            SpawnHeroesInZone(GridZone.Passive);
            
            // Spawn in Active zone
            SpawnHeroesInZone(GridZone.Active);
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Spawned {_allHeroes.Count} test heroes in Passive and Active zones");
            }
        }
        
        /// <summary>
        /// Spawns heroes in all empty slots of the specified zone
        /// </summary>
        private void SpawnHeroesInZone(GridZone zone)
        {
            List<GridSlot> emptySlots = GridManager.Instance.GetEmptySlots(zone);
            
            foreach (GridSlot slot in emptySlots)
            {
                SpawnHeroAtSlot(slot);
            }
        }
        
        /// <summary>
        /// Spawns a hero at the specified grid slot
        /// </summary>
        private Hero SpawnHeroAtSlot(GridSlot slot)
        {
            if (slot == null || slot.IsOccupied)
            {
                Debug.LogWarning("[HeroQueueManager] Cannot spawn hero - slot is null or occupied");
                return null;
            }
            
            // Get world position from GridManager
            Vector3 spawnPosition = GridManager.Instance.GridToWorldPosition(slot.Zone, slot.Row, slot.Column);
            
            // Spawn hero prefab
            GameObject heroObj = Instantiate(TestHeroPrefab, spawnPosition, Quaternion.identity);
            heroObj.name = $"Hero_{slot.Zone}_{slot.Row}_{slot.Column}";
            
            // Get Hero component
            Hero hero = heroObj.GetComponent<Hero>();
            if (hero == null)
            {
                Debug.LogError($"[HeroQueueManager] TestHeroPrefab is missing Hero component!");
                Destroy(heroObj);
                return null;
            }
            
            // Place hero in grid
            bool placed = GridManager.Instance.PlaceHero(hero, slot.Zone, slot.Row, slot.Column);
            
            if (!placed)
            {
                Debug.LogWarning($"[HeroQueueManager] Failed to place hero at {slot.Zone} ({slot.Row}, {slot.Column})");
                Destroy(heroObj);
                return null;
            }
            
            // Track hero
            _allHeroes.Add(hero);
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Spawned hero at {slot.Zone} ({slot.Row}, {slot.Column})");
            }
            
            return hero;
        }
        
        /// <summary>
        /// Called when a hero in Active zone is clicked
        /// </summary>
        public void OnHeroClicked(Hero hero)
        {
            // Block input during animations
            if (_isAnimating)
            {
                if (ShowDebugLogs)
                {
                    Debug.LogWarning("[HeroQueueManager] Animation in progress, click blocked");
                }
                return;
            }
            
            if (hero == null || !hero.IsInActiveZone)
            {
                if (ShowDebugLogs)
                {
                    Debug.LogWarning("[HeroQueueManager] Clicked hero is null or not in Active zone");
                }
                return;
            }
            
            // Deselect previous hero
            if (SelectedHero != null)
            {
                SelectedHero.Unhighlight();
            }
            
            // Select this hero
            SelectedHero = hero;
            SelectedHero.Highlight();
            
            int lane = GridManager.Instance.GetHeroLane(hero);
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Selected hero: {hero.HeroName} at Active ({hero.CurrentGridSlot.Row}, {hero.CurrentGridSlot.Column}) Lane {lane}");
            }
            
            // Deploy to Firing zone and trigger lane shift
            DeploySelectedHeroToFiring();
        }
        
        /// <summary>
        /// Deploys the selected hero to the Firing zone and triggers lane shift if enabled
        /// </summary>
        public void DeploySelectedHeroToFiring()
        {
            if (SelectedHero == null)
            {
                if (ShowDebugLogs)
                {
                    Debug.LogWarning("[HeroQueueManager] No hero selected for deployment");
                }
                return;
            }
            
            // Find leftmost empty slot in Firing zone (left-to-right, top-to-bottom order)
            GridSlot emptyFiringSlot = GridManager.Instance.GetLeftmostEmptySlot(GridZone.Firing);
            
            if (emptyFiringSlot == null)
            {
                if (ShowDebugLogs)
                {
                    Debug.LogWarning("[HeroQueueManager] No empty slots in Firing zone!");
                }
                SelectedHero.Unhighlight();
                SelectedHero = null;
                return;
            }
            
            // Store lane index before removal
            int deployedLane = GridManager.Instance.GetHeroLane(SelectedHero);
            
            // Remove hero from Active zone
            GridSlot oldSlot = SelectedHero.CurrentGridSlot;
            bool removed = GridManager.Instance.RemoveHero(SelectedHero);
            
            if (!removed)
            {
                Debug.LogError("[HeroQueueManager] Failed to remove hero from Active zone!");
                SelectedHero.Unhighlight();
                SelectedHero = null;
                return;
            }
            
            // Place hero in Firing zone
            bool placed = GridManager.Instance.PlaceHero(SelectedHero, emptyFiringSlot.Zone, emptyFiringSlot.Row, emptyFiringSlot.Column);
            
            if (!placed)
            {
                Debug.LogError("[HeroQueueManager] Failed to place hero in Firing zone!");
                // Try to restore to original slot
                GridManager.Instance.PlaceHero(SelectedHero, oldSlot.Zone, oldSlot.Row, oldSlot.Column);
                SelectedHero.Unhighlight();
                SelectedHero = null;
                return;
            }
            
            // Move hero GameObject to new position (instant)
            Vector3 newPosition = GridManager.Instance.GridToWorldPosition(emptyFiringSlot.Zone, emptyFiringSlot.Row, emptyFiringSlot.Column);
            SelectedHero.transform.position = newPosition;
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Deployed {SelectedHero.HeroName} from Active Lane {deployedLane} ({oldSlot.Row}, {oldSlot.Column}) → Firing ({emptyFiringSlot.Row}, {emptyFiringSlot.Column})");
            }
            
            // Unhighlight and clear selection
            SelectedHero.Unhighlight();
            SelectedHero = null;
            
            // Trigger lane shift if enabled
            if (EnableLaneQueueSystem && deployedLane >= 0)
            {
                StartCoroutine(ShiftLaneUpCoroutine(deployedLane));
            }
        }
        
        /// <summary>
        /// Clears all spawned heroes (for testing)
        /// </summary>
        public void ClearAllHeroes()
        {
            foreach (Hero hero in _allHeroes)
            {
                if (hero != null)
                {
                    GridManager.Instance.RemoveHero(hero);
                    Destroy(hero.gameObject);
                }
            }
            
            _allHeroes.Clear();
            SelectedHero = null;
            
            if (ShowDebugLogs)
            {
                Debug.Log("[HeroQueueManager] Cleared all heroes");
            }
        }
        
        /// <summary>
        /// Get all currently spawned heroes
        /// </summary>
        public List<Hero> GetAllHeroes()
        {
            return _allHeroes;
        }
        
        /// <summary>
        /// Get heroes in a specific zone
        /// </summary>
        public List<Hero> GetHeroesInZone(GridZone zone)
        {
            return _allHeroes.FindAll(h => h.CurrentGridSlot?.Zone == zone);
        }
        
        #region Lane Shifting System
        
        /// <summary>
        /// Coroutine to shift all heroes in a lane upward with animation
        /// </summary>
        private IEnumerator ShiftLaneUpCoroutine(int laneIndex)
        {
            _isAnimating = true;
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Starting lane shift for Lane {laneIndex}");
            }
            
            // Wait for deployment delay
            yield return new WaitForSeconds(AnimationDelay);
            
            // Build shift plan (which heroes move where)
            List<HeroShiftData> shiftPlan = BuildLaneShiftPlan(laneIndex);
            
            if (shiftPlan.Count == 0)
            {
                if (ShowDebugLogs)
                {
                    Debug.Log($"[HeroQueueManager] Lane {laneIndex} has no heroes to shift");
                }
                _isAnimating = false;
                yield break;
            }
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Lane {laneIndex} shifting {shiftPlan.Count} heroes");
            }
            
            // Start all animations simultaneously
            foreach (var shiftData in shiftPlan)
            {
                StartCoroutine(AnimateHeroMovement(shiftData));
            }
            
            // Wait for animations to complete
            yield return new WaitForSeconds(AnimationDuration);
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Lane {laneIndex} shift complete");
            }
            
            _isAnimating = false;
        }
        
        /// <summary>
        /// Build a plan for shifting heroes in a specific lane
        /// </summary>
        private List<HeroShiftData> BuildLaneShiftPlan(int laneIndex)
        {
            List<HeroShiftData> plan = new List<HeroShiftData>();
            
            // Check Active slot in this lane
            GridSlot activeSlot = GridManager.Instance.GetSlot(GridZone.Active, 0, laneIndex);
            
            if (activeSlot == null || activeSlot.IsOccupied)
            {
                // Active slot is occupied or doesn't exist, no shift needed
                return plan;
            }
            
            // Get all heroes in Passive lane (this column only)
            List<Hero> passiveHeroes = GridManager.Instance.GetLaneHeroes(GridZone.Passive, laneIndex);
            
            if (passiveHeroes.Count == 0)
            {
                // No heroes in Passive lane, nothing to shift
                return plan;
            }
            
            // Shift logic: Each hero moves up one row
            // Passive Row 0 → Active
            // Passive Row 1 → Passive Row 0
            // etc.
            
            // IMPORTANT: We need to process from bottom to top to avoid checking slots
            // that will be emptied by heroes above them in the shift
            
            int passiveRows = GridManager.Instance.GetRowCount(GridZone.Passive);
            
            // Process rows from bottom to top (reverse order)
            for (int row = passiveRows - 1; row >= 0; row--)
            {
                GridSlot currentSlot = GridManager.Instance.GetSlot(GridZone.Passive, row, laneIndex);
                
                if (currentSlot == null || !currentSlot.IsOccupied)
                {
                    continue; // Empty slot, skip
                }
                
                Hero hero = currentSlot.OccupyingHero;
                GridSlot targetSlot = null;
                
                if (row == 0)
                {
                    // Top row of Passive → Move to Active
                    targetSlot = activeSlot;
                }
                else
                {
                    // Other rows → Move up one row within Passive
                    targetSlot = GridManager.Instance.GetSlot(GridZone.Passive, row - 1, laneIndex);
                }
                
                // Since we're processing bottom-to-top, we know the target slot will be
                // vacated by the hero currently there (who will also shift up)
                // So we don't need to check IsEmpty - just verify slot exists
                if (targetSlot != null)
                {
                    plan.Add(new HeroShiftData
                    {
                        Hero = hero,
                        FromSlot = currentSlot,
                        ToSlot = targetSlot,
                        StartPosition = hero.transform.position,
                        EndPosition = GridManager.Instance.GridToWorldPosition(targetSlot.Zone, targetSlot.Row, targetSlot.Column)
                    });
                }
            }
            
            return plan;
        }
        
        /// <summary>
        /// Animate a single hero's movement with Lerp
        /// </summary>
        private IEnumerator AnimateHeroMovement(HeroShiftData shiftData)
        {
            Hero hero = shiftData.Hero;
            GridSlot fromSlot = shiftData.FromSlot;
            GridSlot toSlot = shiftData.ToSlot;
            Vector3 startPos = shiftData.StartPosition;
            Vector3 endPos = shiftData.EndPosition;
            
            // Remove hero from current slot
            bool removed = GridManager.Instance.RemoveHero(hero);
            if (!removed)
            {
                Debug.LogError($"[HeroQueueManager] Failed to remove {hero.HeroName} from slot during shift");
                yield break;
            }
            
            // Animate movement
            float elapsed = 0f;
            while (elapsed < AnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / AnimationDuration;
                hero.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            // Ensure final position
            hero.transform.position = endPos;
            
            // Place hero in new slot
            bool placed = GridManager.Instance.PlaceHero(hero, toSlot.Zone, toSlot.Row, toSlot.Column);
            if (!placed)
            {
                Debug.LogError($"[HeroQueueManager] Failed to place {hero.HeroName} in new slot during shift");
            }
            
            if (ShowDebugLogs)
            {
                Debug.Log($"[HeroQueueManager] Moved {hero.HeroName} from {fromSlot.Zone}({fromSlot.Row},{fromSlot.Column}) → {toSlot.Zone}({toSlot.Row},{toSlot.Column})");
            }
        }
        
        /// <summary>
        /// Data structure for hero shift operations
        /// </summary>
        private class HeroShiftData
        {
            public Hero Hero;
            public GridSlot FromSlot;
            public GridSlot ToSlot;
            public Vector3 StartPosition;
            public Vector3 EndPosition;
        }
        
        #endregion
    }
}
