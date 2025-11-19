using UnityEngine;
using ProjectBlast.Grid;
using ProjectBlast.Heroes;

namespace ProjectBlast.Testing
{
    /// <summary>
    /// GridManagerTester - Test script for validating GridManager functionality.
    /// Attach this to a GameObject in your scene to run tests.
    /// 
    /// USAGE:
    /// 1. Create empty GameObject named "GridManagerTester"
    /// 2. Attach this script
    /// 3. Assign test hero prefab (or create a simple cube)
    /// 4. Press Play and use keyboard to test:
    ///    - Space: Place random hero
    ///    - R: Remove random hero
    ///    - C: Clear all grids
    ///    - L: Log grid status
    /// </summary>
    public class GridManagerTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Tooltip("Hero prefab to spawn for testing (must have Hero component)")]
        public GameObject TestHeroPrefab;
        
        [Tooltip("Should automatically run tests on start?")]
        public bool AutoRunTests = false;
        
        [Header("Runtime Controls")]
        [Tooltip("Press Space to place hero")]
        public KeyCode PlaceHeroKey = KeyCode.Space;
        
        [Tooltip("Press R to remove hero")]
        public KeyCode RemoveHeroKey = KeyCode.R;
        
        [Tooltip("Press C to clear all grids")]
        public KeyCode ClearGridsKey = KeyCode.C;
        
        [Tooltip("Press L to log grid status")]
        public KeyCode LogStatusKey = KeyCode.L;
        
        private int _heroCounter = 0;
        
        void Start()
        {
            if (AutoRunTests)
            {
                RunAutomatedTests();
            }
            else
            {
                Debug.Log("[GridManagerTester] Ready! Use keyboard controls:\n" +
                          $"  {PlaceHeroKey} = Place hero\n" +
                          $"  {RemoveHeroKey} = Remove hero\n" +
                          $"  {ClearGridsKey} = Clear grids\n" +
                          $"  {LogStatusKey} = Log status");
            }
        }
        
        void Update()
        {
            if (Input.GetKeyDown(PlaceHeroKey))
            {
                TestPlaceRandomHero();
            }
            
            if (Input.GetKeyDown(RemoveHeroKey))
            {
                TestRemoveRandomHero();
            }
            
            if (Input.GetKeyDown(ClearGridsKey))
            {
                TestClearAllGrids();
            }
            
            if (Input.GetKeyDown(LogStatusKey))
            {
                LogGridStatus();
            }
        }
        
        #region Automated Tests
        
        void RunAutomatedTests()
        {
            Debug.Log("=== GridManager Automated Tests ===");
            
            TestGridInitialization();
            TestPlacementAndRemoval();
            TestQueries();
            TestValidation();
            TestSpatialConversion();
            
            Debug.Log("=== All Tests Complete ===");
        }
        
        void TestGridInitialization()
        {
            Debug.Log("\n--- Test: Grid Initialization ---");
            
            var allPassiveSlots = GridManager.Instance.GetAllSlots(GridZone.Passive);
            var allActiveSlots = GridManager.Instance.GetAllSlots(GridZone.Active);
            var allFiringSlots = GridManager.Instance.GetAllSlots(GridZone.Firing);
            
            Debug.Log($"✓ Passive Grid: {allPassiveSlots.Count} slots " +
                      $"({GridManager.Instance.PassiveRows}x{GridManager.Instance.PassiveColumns})");
            Debug.Log($"✓ Active Grid: {allActiveSlots.Count} slots " +
                      $"({GridManager.Instance.ActiveRows}x{GridManager.Instance.ActiveColumns})");
            Debug.Log($"✓ Firing Grid: {allFiringSlots.Count} slots " +
                      $"({GridManager.Instance.FiringRows}x{GridManager.Instance.FiringColumns})");
        }
        
        void TestPlacementAndRemoval()
        {
            Debug.Log("\n--- Test: Placement & Removal ---");
            
            // Create test hero
            var hero = CreateTestHero("TestHero");
            
            // Test placement
            bool placed = GridManager.Instance.PlaceHero(hero, GridZone.Firing, 0, 0);
            Debug.Log($"✓ Place hero at Firing[0,0]: {(placed ? "Success" : "Failed")}");
            
            // Test duplicate placement (should fail)
            var hero2 = CreateTestHero("TestHero2");
            bool placedDuplicate = GridManager.Instance.PlaceHero(hero2, GridZone.Firing, 0, 0);
            Debug.Log($"✓ Place second hero at same slot: {(placedDuplicate ? "Failed (unexpected)" : "Correctly rejected")}");
            
            // Test removal
            bool removed = GridManager.Instance.RemoveHero(hero);
            Debug.Log($"✓ Remove hero: {(removed ? "Success" : "Failed")}");
            
            // Cleanup
            if (hero != null) Destroy(hero.gameObject);
            if (hero2 != null) Destroy(hero2.gameObject);
        }
        
        void TestQueries()
        {
            Debug.Log("\n--- Test: Queries ---");
            
            // Place some heroes
            var hero1 = CreateTestHero("QueryTest1");
            var hero2 = CreateTestHero("QueryTest2");
            
            GridManager.Instance.PlaceHero(hero1, GridZone.Active, 0, 0);
            GridManager.Instance.PlaceHero(hero2, GridZone.Active, 1, 1);
            
            var occupiedSlots = GridManager.Instance.GetOccupiedSlots(GridZone.Active);
            var emptySlots = GridManager.Instance.GetEmptySlots(GridZone.Active);
            var heroes = GridManager.Instance.GetAllHeroesInZone(GridZone.Active);
            
            Debug.Log($"✓ Occupied slots in Active: {occupiedSlots.Count}");
            Debug.Log($"✓ Empty slots in Active: {emptySlots.Count}");
            Debug.Log($"✓ Heroes in Active: {heroes.Count}");
            
            // Cleanup
            GridManager.Instance.RemoveHero(hero1);
            GridManager.Instance.RemoveHero(hero2);
            if (hero1 != null) Destroy(hero1.gameObject);
            if (hero2 != null) Destroy(hero2.gameObject);
        }
        
        void TestValidation()
        {
            Debug.Log("\n--- Test: Validation ---");
            
            bool validPos = GridManager.Instance.IsValidPosition(GridZone.Firing, 0, 0);
            bool invalidPos = GridManager.Instance.IsValidPosition(GridZone.Firing, 99, 99);
            
            Debug.Log($"✓ Valid position check [0,0]: {validPos}");
            Debug.Log($"✓ Invalid position check [99,99]: {!invalidPos}");
            
            var hero = CreateTestHero("ValidationTest");
            GridManager.Instance.PlaceHero(hero, GridZone.Passive, 0, 0);
            
            bool isEmpty = GridManager.Instance.IsSlotEmpty(GridZone.Passive, 0, 0);
            bool isOccupied = GridManager.Instance.IsSlotOccupied(GridZone.Passive, 0, 0);
            
            Debug.Log($"✓ Slot empty check (should be false): {!isEmpty}");
            Debug.Log($"✓ Slot occupied check (should be true): {isOccupied}");
            
            // Cleanup
            GridManager.Instance.RemoveHero(hero);
            if (hero != null) Destroy(hero.gameObject);
        }
        
        void TestSpatialConversion()
        {
            Debug.Log("\n--- Test: Spatial Conversion ---");
            
            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(GridZone.Firing, 1, 1);
            Debug.Log($"✓ Grid [1,1] to world: {worldPos}");
            
            var (row, col) = GridManager.Instance.WorldToGridPosition(GridZone.Firing, worldPos);
            Debug.Log($"✓ World {worldPos} back to grid: [{row},{col}]");
            
            var nearestSlot = GridManager.Instance.GetNearestEmptySlot(GridZone.Active, Vector3.zero);
            Debug.Log($"✓ Nearest empty slot to origin: {nearestSlot?.GetCoordinateLabel()}");
        }
        
        #endregion
        
        #region Manual Tests
        
        void TestPlaceRandomHero()
        {
            var hero = CreateTestHero($"Hero_{_heroCounter++}");
            
            // Randomly pick a zone
            GridZone zone = (GridZone)Random.Range(0, 3);
            
            bool success = GridManager.Instance.TryPlaceHeroInZone(hero, zone);
            
            if (success)
            {
                Debug.Log($"✓ Placed {hero.HeroName} in {zone} zone at {hero.CurrentGridSlot.GetCoordinateLabel()}");
            }
            else
            {
                Debug.LogWarning($"✗ Failed to place {hero.HeroName} in {zone} zone (no empty slots?)");
                Destroy(hero.gameObject);
            }
        }
        
        void TestRemoveRandomHero()
        {
            // Try each zone
            foreach (GridZone zone in System.Enum.GetValues(typeof(GridZone)))
            {
                var heroes = GridManager.Instance.GetAllHeroesInZone(zone);
                if (heroes.Count > 0)
                {
                    var heroToRemove = heroes[Random.Range(0, heroes.Count)];
                    GridManager.Instance.RemoveHero(heroToRemove);
                    Destroy(heroToRemove.gameObject);
                    Debug.Log($"✓ Removed hero from {zone} zone");
                    return;
                }
            }
            
            Debug.LogWarning("✗ No heroes to remove");
        }
        
        void TestClearAllGrids()
        {
            int removedCount = 0;
            
            foreach (GridZone zone in System.Enum.GetValues(typeof(GridZone)))
            {
                var heroes = GridManager.Instance.GetAllHeroesInZone(zone);
                foreach (var hero in heroes)
                {
                    GridManager.Instance.RemoveHero(hero);
                    Destroy(hero.gameObject);
                    removedCount++;
                }
            }
            
            Debug.Log($"✓ Cleared all grids, removed {removedCount} heroes");
        }
        
        void LogGridStatus()
        {
            Debug.Log("\n=== Grid Status ===");
            
            foreach (GridZone zone in System.Enum.GetValues(typeof(GridZone)))
            {
                int occupied = GridManager.Instance.GetOccupiedCount(zone);
                int empty = GridManager.Instance.GetEmptyCount(zone);
                int total = occupied + empty;
                
                Debug.Log($"{zone} Grid: {occupied}/{total} occupied, {empty} empty");
                
                var heroes = GridManager.Instance.GetAllHeroesInZone(zone);
                foreach (var hero in heroes)
                {
                    Debug.Log($"  - {hero.HeroName} at {hero.CurrentGridSlot.GetCoordinateLabel()}");
                }
            }
            
            Debug.Log("==================");
        }
        
        #endregion
        
        #region Helper Methods
        
        Hero CreateTestHero(string heroName)
        {
            GameObject heroObj;
            
            if (TestHeroPrefab != null)
            {
                heroObj = Instantiate(TestHeroPrefab);
            }
            else
            {
                // Create a simple cube if no prefab is assigned
                heroObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                heroObj.transform.localScale = Vector3.one * 0.8f;
                
                // Add Hero component if missing
                if (heroObj.GetComponent<Hero>() == null)
                {
                    heroObj.AddComponent<Hero>();
                }
            }
            
            heroObj.name = heroName;
            var hero = heroObj.GetComponent<Hero>();
            hero.HeroName = heroName;
            
            return hero;
        }
        
        #endregion
    }
}
