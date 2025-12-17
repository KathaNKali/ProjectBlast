# ProjectBlast - Game Architecture (TDE Pattern)

**Last Updated:** December 17, 2025

This document defines the complete game architecture for ProjectBlast, following **TopDown Engine (TDE) best practices** for a production-ready mobile game with home screen, deck building, hero collection, level progression, and combat systems.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Core Singleton Systems](#core-singleton-systems)
3. [Scene Structure](#scene-structure)
4. [Data Management](#data-management)
5. [UI Architecture](#ui-architecture)
6. [Gameplay Loop](#gameplay-loop)
7. [Save System](#save-system)
8. [Event-Driven Communication](#event-driven-communication)
9. [Implementation Checklist](#implementation-checklist)

---

## Architecture Overview

### TDE Pattern Philosophy

TopDown Engine uses a **persistent singleton pattern** for cross-scene management:

```
┌─────────────────────────────────────────────────────────────┐
│  PERSISTENT LAYER (DontDestroyOnLoad)                        │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐│
│  │  GameManager   │  │ MMPersistence  │  │ MMSoundManager ││
│  │ (Global State) │  │   Manager      │  │  (Audio)       ││
│  └────────────────┘  └────────────────┘  └────────────────┘│
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐│
│  │ PlayerProgress │  │ HeroCollection │  │ DeckManager    ││
│  │    Manager     │  │    Manager     │  │  (Custom)      ││
│  └────────────────┘  └────────────────┘  └────────────────┘│
└─────────────────────────────────────────────────────────────┘
                            ↓ ↓ ↓
┌─────────────────────────────────────────────────────────────┐
│  SCENE LAYER (Scene-Specific, Destroyed on Load)             │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐│
│  │  LevelManager  │  │   GUIManager   │  │ InputManager   ││
│  │ (Scene Setup)  │  │  (Scene UI)    │  │ (Per Scene)    ││
│  └────────────────┘  └────────────────┘  └────────────────┘│
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐│
│  │    GridManager │  │ CombatCoord.   │  │ WaveManager    ││
│  │  (Grid Combat) │  │ (Allocation)   │  │ (Enemy Spawn)  ││
│  └────────────────┘  └────────────────┘  └────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

### Key Principles

1. **MMPersistentSingleton** - Global managers (GameManager, save data, audio)
2. **MMSingleton** - Scene-specific managers (LevelManager, GUIManager, GridManager)
3. **MMEventManager** - Decoupled event-driven communication (no direct references)
4. **ScriptableObjects** - Data-driven design (Hero stats, weapons, levels)
5. **MMPersistenceManager** - Save/load system for player progress

---

## Core Singleton Systems

### 1. Persistent Managers (DontDestroyOnLoad)

#### **ProjectBlastGameManager** (extends GameManager)
```csharp
public class ProjectBlastGameManager : GameManager
{
    // Global game state
    public int Currency;                    // Soft currency (coins)
    public int PremiumCurrency;             // Hard currency (gems)
    public int CurrentLevel;                // Level progression
    public int CurrentWave;                 // Wave within level
    
    // Session state
    public GameState CurrentGameState;      // Menu, Combat, Victory, Defeat
    public DeckConfiguration ActiveDeck;    // Current hero deck
    
    // Methods
    public void StartLevel(int levelIndex);
    public void CompleteLevel(LevelResults results);
    public void AddCurrency(int amount, CurrencyType type);
    public void SpendCurrency(int amount, CurrencyType type);
}

public enum GameState
{
    Boot,           // Initial loading
    MainMenu,       // Home screen
    DeckBuilding,   // Deck configuration
    HeroCollection, // Hero management
    Combat,         // Active gameplay
    Victory,        // Level complete
    Defeat,         // Level failed
    Shop,           // Store
    Settings        // Options menu
}
```

#### **PlayerProgressManager** (MMPersistentSingleton)
```csharp
public class PlayerProgressManager : MMPersistentSingleton<PlayerProgressManager>
{
    // Progression data
    public PlayerData CurrentPlayer;        // All player data
    public List<LevelProgress> Levels;      // Level completion status
    public Dictionary<string, bool> Achievements;
    
    // Methods
    public void UnlockLevel(int levelIndex);
    public void RecordLevelCompletion(int level, int stars);
    public bool IsLevelUnlocked(int levelIndex);
    public LevelProgress GetLevelProgress(int levelIndex);
    public void SaveProgress();             // Trigger MMPersistenceManager save
}

[System.Serializable]
public class PlayerData
{
    public string PlayerName;
    public int PlayerLevel;
    public int Experience;
    public int HighestLevelUnlocked;
    public DateTime LastPlayedDate;
}

[System.Serializable]
public class LevelProgress
{
    public int LevelIndex;
    public int Stars;                       // 0-3 stars
    public int HighScore;
    public int TimesPlayed;
    public int TimesCompleted;
    public bool PerfectClear;               // No hero deaths
}
```

#### **HeroCollectionManager** (MMPersistentSingleton)
```csharp
public class HeroCollectionManager : MMPersistentSingleton<HeroCollectionManager>
{
    // Hero roster
    public List<OwnedHero> OwnedHeroes;     // Player's hero collection
    public List<HeroDataSO> AllHeroes;      // Reference to all available heroes
    
    // Methods
    public void UnlockHero(string heroID);
    public void UpgradeHero(string heroID);
    public OwnedHero GetHero(string heroID);
    public bool IsHeroOwned(string heroID);
    public List<OwnedHero> GetOwnedHeroes();
}

[System.Serializable]
public class OwnedHero
{
    public string HeroID;                   // Reference to HeroDataSO
    public int Level;                       // Hero level (1-max)
    public int Experience;                  // Hero XP
    public DateTime UnlockedDate;
    public int TimesUsed;
    public int TotalKills;
    public bool IsFavorite;
    
    // Computed properties
    public HeroDataSO GetHeroData() => /* Load from Resources */;
}
```

#### **DeckManager** (MMPersistentSingleton)
```csharp
public class DeckManager : MMPersistentSingleton<DeckManager>
{
    // Deck configurations
    public List<DeckConfiguration> SavedDecks;
    public DeckConfiguration ActiveDeck;
    public int MaxDeckSlots = 5;            // Max heroes per deck
    
    // Methods
    public void SetActiveDeck(DeckConfiguration deck);
    public void SaveDeck(DeckConfiguration deck);
    public void DeleteDeck(int deckIndex);
    public bool ValidateDeck(DeckConfiguration deck);
    public DeckConfiguration CreateNewDeck();
}

[System.Serializable]
public class DeckConfiguration
{
    public string DeckName;
    public List<string> HeroIDs;            // Ordered list (max 5)
    public DateTime LastModified;
    
    // Validation
    public bool IsValid() => HeroIDs.Count > 0 && HeroIDs.Count <= 5;
}
```

#### **MMPersistenceManager** (TDE Built-in)
- Already implemented by TDE
- Handles save/load to disk
- Use `IMMPersistent` interface on managers that need saving
- Events: `MMGameEvent.SaveToMemory`, `MMGameEvent.LoadFromMemory`

---

### 2. Scene-Specific Managers (MMSingleton)

#### **LevelManager** (TDE Built-in, Extend)
```csharp
public class ProjectBlastLevelManager : LevelManager
{
    // Combat-specific extensions
    public GridManager GridManager;
    public WaveManager WaveManager;
    public CombatCoordinator CombatCoordinator;
    
    // Level configuration
    public LevelDataSO CurrentLevelData;
    
    protected override void Start()
    {
        base.Start();
        InitializeCombatLevel();
    }
    
    public void InitializeCombatLevel()
    {
        // Spawn heroes from active deck
        SpawnHeroesFromDeck();
        // Initialize wave system
        WaveManager.StartWaves();
    }
    
    public void SpawnHeroesFromDeck()
    {
        DeckConfiguration deck = DeckManager.Instance.ActiveDeck;
        foreach (string heroID in deck.HeroIDs)
        {
            SpawnHero(heroID);
        }
    }
}
```

#### **GUIManager** (TDE Built-in, Extend)
```csharp
public class ProjectBlastGUIManager : GUIManager
{
    // Combat UI
    public HeroQueueDisplay HeroQueueDisplay;
    public WaveProgressDisplay WaveDisplay;
    public CurrencyDisplay CurrencyDisplay;
    public PauseMenu PauseMenu;
    
    // HUD updates (called via events)
    public void UpdateWaveProgress(int current, int total);
    public void UpdateCurrency(int amount);
    public void ShowVictoryScreen(LevelResults results);
    public void ShowDefeatScreen();
}
```

#### **WaveManager** (Custom, MMSingleton)
```csharp
public class WaveManager : MMSingleton<WaveManager>, MMEventListener<MMLifeCycleEvent>
{
    // Wave configuration
    public LevelDataSO LevelData;
    public List<WaveData> Waves;
    public int CurrentWave = 0;
    public WaveState State = WaveState.Waiting;
    
    // Spawning
    public float WaveStartDelay = 2f;
    public float EnemySpawnInterval = 1f;
    
    public void StartWaves();
    public void SpawnNextWave();
    public void OnEnemyDied(MMLifeCycleEvent evt);
    public void CheckWaveCompletion();
    
    // Events triggered
    // - WaveStartedEvent
    // - WaveCompletedEvent
    // - AllWavesCompletedEvent (Victory)
}

public enum WaveState { Waiting, Spawning, Active, Complete }
```

#### **CombatCoordinator** (Already Implemented)
- Bullet allocation system
- Hero ammo tracking
- Cooperative targeting
- Event-driven architecture (Phase 1-6 complete)

#### **GridManager** (Already Exists)
- Grid-based movement
- Lane management
- Position validation

---

## Scene Structure

### Scene Hierarchy

```
1. BootScene (Initial Load)
   - Purpose: Initialize persistent managers, load player data
   - Managers: ProjectBlastGameManager, MMPersistenceManager, MMSoundManager
   - Flow: Load save → MainMenuScene

2. MainMenuScene (Home Screen)
   - UI: Play Button, Hero Collection, Deck Builder, Shop, Settings
   - Managers: MenuUIManager (scene-specific)
   - Flow: Player selects action → Navigate to appropriate scene

3. HeroCollectionScene
   - UI: Hero grid, filters, stats display, upgrade buttons
   - Managers: HeroCollectionUIManager
   - Data: HeroCollectionManager.OwnedHeroes
   - Flow: Browse/upgrade heroes → Back to MainMenu

4. DeckBuildingScene
   - UI: Deck slots (5), hero selector, save/load decks
   - Managers: DeckBuilderUIManager
   - Data: DeckManager, HeroCollectionManager
   - Validation: Check deck validity before saving
   - Flow: Configure deck → Start Level or Back to MainMenu

5. LevelSelectScene
   - UI: Level buttons (grid/map), stars, locked/unlocked
   - Managers: LevelSelectUIManager
   - Data: PlayerProgressManager.Levels
   - Flow: Select level → CombatScene

6. CombatScene (Gameplay)
   - Managers: ProjectBlastLevelManager, GridManager, WaveManager, 
              CombatCoordinator, GUIManager, InputManager
   - Flow: Spawn heroes → Waves start → Victory/Defeat → ResultsScene

7. ResultsScene (Victory/Defeat)
   - UI: Stars earned, currency rewards, level progress
   - Data: Update PlayerProgressManager, HeroCollectionManager (hero XP)
   - Flow: Show rewards → MainMenu or Retry
```

---

## Data Management

### ScriptableObject Architecture

#### **HeroDataSO** (Hero Definition)
```csharp
[CreateAssetMenu(fileName = "Hero_", menuName = "ProjectBlast/Hero Data")]
public class HeroDataSO : ScriptableObject
{
    [Header("Identity")]
    public string HeroID;
    public string HeroName;
    public Sprite Portrait;
    public GameObject HeroPrefab;
    public HeroClass Class;
    
    [Header("Base Stats (Level 1)")]
    public int BaseHealth;
    public int BaseDamage;
    public float BaseAttackSpeed;
    public int BaseAmmo;
    public bool UnlimitedAmmo;
    
    [Header("Scaling")]
    public float HealthPerLevel;
    public float DamagePerLevel;
    
    [Header("Unlock Requirements")]
    public int UnlockCost;
    public CurrencyType UnlockCurrency;
    public int RequiredPlayerLevel;
    
    // Computed stats based on hero level
    public int GetHealth(int level) => BaseHealth + (int)(HealthPerLevel * (level - 1));
    public int GetDamage(int level) => BaseDamage + (int)(DamagePerLevel * (level - 1));
}

public enum HeroClass { Warrior, Ranger, Mage, Support, Tank }
public enum CurrencyType { Coins, Gems }
```

#### **WeaponDataSO** (Weapon Definition)
```csharp
[CreateAssetMenu(fileName = "Weapon_", menuName = "ProjectBlast/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    public string WeaponID;
    public string WeaponName;
    public GameObject WeaponPrefab;
    public int Damage;
    public float FireRate;
    public int MagazineSize;
    public ProjectileType ProjectileType;
}
```

#### **LevelDataSO** (Level Configuration)
```csharp
[CreateAssetMenu(fileName = "Level_", menuName = "ProjectBlast/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [Header("Level Info")]
    public int LevelIndex;
    public string LevelName;
    public Sprite LevelIcon;
    public string SceneName;
    
    [Header("Unlock Requirements")]
    public int RequiredPlayerLevel;
    public int PreviousLevelRequired;
    
    [Header("Waves")]
    public List<WaveData> Waves;
    
    [Header("Rewards")]
    public int CoinReward;
    public int ExperienceReward;
    public RewardData[] StarRewards;        // Rewards for 1/2/3 stars
    
    [Header("Star Requirements")]
    public int OneStarKills;
    public int TwoStarKills;
    public int ThreeStarKills;
    public bool ThreeStarRequiresPerfect;   // No hero deaths
}

[System.Serializable]
public class WaveData
{
    public int WaveNumber;
    public List<EnemySpawnData> Enemies;
    public float DelayBeforeNextWave;
}

[System.Serializable]
public class EnemySpawnData
{
    public GameObject EnemyPrefab;
    public int Count;
    public float SpawnInterval;
    public Vector3 SpawnPosition;
}
```

---

## UI Architecture

### TDE UI Pattern (MMUICanvases + Event-Driven)

```csharp
// Example: Main Menu UI Manager
public class MainMenuUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject MainPanel;
    public GameObject HeroCollectionButton;
    public GameObject DeckBuilderButton;
    public GameObject PlayButton;
    
    [Header("Data Display")]
    public TextMeshProUGUI CurrencyText;
    public TextMeshProUGUI PlayerLevelText;
    
    protected virtual void Start()
    {
        RefreshUI();
    }
    
    public virtual void RefreshUI()
    {
        var gameManager = ProjectBlastGameManager.Instance;
        CurrencyText.text = $"Coins: {gameManager.Currency}";
        PlayerLevelText.text = $"Level {PlayerProgressManager.Instance.CurrentPlayer.PlayerLevel}";
    }
    
    // Button callbacks
    public void OnPlayButtonClicked()
    {
        ProjectBlastGameManager.Instance.CurrentGameState = GameState.DeckBuilding;
        MMSceneLoadingManager<MMSceneLoadingManager>.LoadScene("LevelSelectScene");
    }
    
    public void OnHeroCollectionClicked()
    {
        MMSceneLoadingManager<MMSceneLoadingManager>.LoadScene("HeroCollectionScene");
    }
}
```

### UI Event Pattern
```csharp
// Currency updated event
public struct CurrencyEvent
{
    public enum EventType { Added, Spent, Updated }
    public EventType Type;
    public CurrencyType Currency;
    public int Amount;
    public int NewTotal;
    
    static CurrencyEvent e;
    public static void Trigger(EventType type, CurrencyType currency, int amount, int total)
    {
        e.Type = type;
        e.Currency = currency;
        e.Amount = amount;
        e.NewTotal = total;
        MMEventManager.TriggerEvent(e);
    }
}

// UI listens for updates
public class CurrencyDisplay : MonoBehaviour, MMEventListener<CurrencyEvent>
{
    public TextMeshProUGUI CoinText;
    
    void OnEnable() => this.MMEventStartListening<CurrencyEvent>();
    void OnDisable() => this.MMEventStopListening<CurrencyEvent>();
    
    public void OnMMEvent(CurrencyEvent evt)
    {
        if (evt.Currency == CurrencyType.Coins)
        {
            CoinText.text = evt.NewTotal.ToString();
        }
    }
}
```

---

## Gameplay Loop

### Level Start Flow

```
1. Player clicks "Play" in MainMenu
   → Navigate to LevelSelectScene
   
2. Player selects level
   → Check PlayerProgressManager.IsLevelUnlocked(levelIndex)
   → Load LevelDataSO
   → Navigate to CombatScene
   
3. CombatScene.Start()
   → ProjectBlastLevelManager.InitializeCombatLevel()
   → Spawn heroes from DeckManager.ActiveDeck
   → WaveManager.StartWaves()
   
4. Wave Loop
   → WaveManager spawns enemies
   → Heroes target and fire (CombatCoordinator allocation)
   → Track kills, deaths, ammo
   
5. Victory Condition
   → All waves complete AND all enemies dead
   → Calculate stars (kills, no deaths, etc.)
   → Trigger VictoryEvent
   
6. Results
   → Update PlayerProgressManager (level progress, stars)
   → Award currency
   → Award hero XP
   → Save progress (MMPersistenceManager)
   → Navigate to ResultsScene
```

### Combat Flow (Already Implemented)

```
1. Hero spawns
   → Register with CombatCoordinator
   → Initialize HeroAmmo CharacterAbility
   
2. Enemy spawns
   → Auto-register with CombatCoordinator (Phase 6)
   → WaveManager tracks count
   
3. Hero targeting (AIActionShoot3D)
   → FindTarget() → Detect enemy
   → Trigger CombatAllocationEvent.Request
   → CombatCoordinator → Grant/Deny response
   
4. Hero fires
   → CanHeroFireNextBullet() check
   → Weapon.ShootRequest()
   → OnHeroFiredBullet() → Ammo consumption
   
5. Projectile hits
   → Health.Damage()
   → OnBulletHit() → Track hit
   
6. Enemy dies
   → Trigger CombatAllocationEvent.EnemyDied
   → Release all hero allocations
   → WaveManager.CheckWaveCompletion()
```

---

## Save System

### TDE Persistence Pattern

ProjectBlast uses **MMPersistenceManager** (built into TDE) for save/load operations.

#### Implementation Steps

1. **Make managers implement IMMPersistent**:

```csharp
public class PlayerProgressManager : MMPersistentSingleton<PlayerProgressManager>, 
                                      IMMPersistent
{
    public string GetGuid() => "PlayerProgress";
    
    public bool ShouldBeSaved() => true;
    
    public string OnSave()
    {
        PlayerProgressData data = new PlayerProgressData
        {
            PlayerData = CurrentPlayer,
            Levels = Levels,
            Achievements = Achievements
        };
        return JsonUtility.ToJson(data);
    }
    
    public void OnLoad(string data)
    {
        if (string.IsNullOrEmpty(data)) return;
        
        PlayerProgressData loaded = JsonUtility.FromJson<PlayerProgressData>(data);
        CurrentPlayer = loaded.PlayerData;
        Levels = loaded.Levels;
        Achievements = loaded.Achievements;
    }
}

[System.Serializable]
public class PlayerProgressData
{
    public PlayerData PlayerData;
    public List<LevelProgress> Levels;
    public Dictionary<string, bool> Achievements;
}
```

2. **Trigger saves at key moments**:

```csharp
// After level completion
public void CompleteLevel(LevelResults results)
{
    PlayerProgressManager.Instance.RecordLevelCompletion(results.LevelIndex, results.Stars);
    HeroCollectionManager.Instance.AwardHeroXP(results.HeroXP);
    
    // Save to memory
    MMGameEvent.Trigger("SaveToMemory");
    
    // Save to file (async)
    MMGameEvent.Trigger("SaveToFile");
}

// On application quit
void OnApplicationQuit()
{
    MMGameEvent.Trigger("SaveToFile");
}
```

3. **Load on boot**:

```csharp
// BootScene initialization
void Start()
{
    MMGameEvent.Trigger("LoadFromFile");
}
```

### Save File Structure

```json
{
  "PersistenceID": "ProjectBlast",
  "SaveDate": "2025-12-17T10:30:00",
  "SceneDatas": {
    "PlayerProgress": {
      "PlayerData": {
        "PlayerName": "Player1",
        "PlayerLevel": 12,
        "Experience": 5000,
        "HighestLevelUnlocked": 8
      },
      "Levels": [
        { "LevelIndex": 1, "Stars": 3, "HighScore": 150 },
        { "LevelIndex": 2, "Stars": 2, "HighScore": 120 }
      ]
    },
    "HeroCollection": {
      "OwnedHeroes": [
        { "HeroID": "hero_knight", "Level": 5, "Experience": 800 },
        { "HeroID": "hero_archer", "Level": 3, "Experience": 400 }
      ]
    },
    "DeckManager": {
      "SavedDecks": [
        {
          "DeckName": "My Deck",
          "HeroIDs": ["hero_knight", "hero_archer", "hero_mage"]
        }
      ],
      "ActiveDeck": { ... }
    }
  }
}
```

---

## Event-Driven Communication

### Core Events (Extend TDE's MMEventManager)

```csharp
// Level events
public struct LevelEvent
{
    public enum EventType { Started, WaveStarted, WaveCompleted, Victory, Defeat }
    public EventType Type;
    public int LevelIndex;
    public int WaveNumber;
    public LevelResults Results;
    
    static LevelEvent e;
    public static void Trigger(EventType type, int level, int wave = 0, LevelResults results = null)
    {
        e.Type = type;
        e.LevelIndex = level;
        e.WaveNumber = wave;
        e.Results = results;
        MMEventManager.TriggerEvent(e);
    }
}

// Hero events
public struct HeroEvent
{
    public enum EventType { Spawned, Died, AmmoChanged, LeveledUp }
    public EventType Type;
    public string HeroID;
    public GameObject HeroObject;
    public int NewLevel;
    
    static HeroEvent e;
    public static void Trigger(EventType type, string heroID, GameObject hero = null, int level = 0)
    {
        e.Type = type;
        e.HeroID = heroID;
        e.HeroObject = hero;
        e.NewLevel = level;
        MMEventManager.TriggerEvent(e);
    }
}

// Currency events (already defined in UI section)
public struct CurrencyEvent { ... }

// Progress events
public struct ProgressEvent
{
    public enum EventType { LevelUnlocked, HeroUnlocked, AchievementUnlocked }
    public EventType Type;
    public int LevelIndex;
    public string HeroID;
    public string AchievementID;
    
    static ProgressEvent e;
    public static void Trigger(EventType type, int level = 0, string id = null)
    {
        e.Type = type;
        e.LevelIndex = level;
        e.HeroID = id;
        e.AchievementID = id;
        MMEventManager.TriggerEvent(e);
    }
}
```

### Event Flow Example: Level Victory

```
1. WaveManager detects all enemies dead
   → LevelEvent.Trigger(EventType.Victory, levelIndex, results)

2. ProjectBlastLevelManager listens
   → Calculate stars
   → Update PlayerProgressManager
   → Award currency
   → CurrencyEvent.Trigger(EventType.Added, coins, amount)

3. GUIManager listens
   → Show victory screen with results

4. HeroCollectionManager listens
   → Award hero XP
   → Check for level-ups
   → HeroEvent.Trigger(EventType.LeveledUp, heroID, newLevel)

5. Save system triggers
   → MMGameEvent.Trigger("SaveToMemory")
   → MMGameEvent.Trigger("SaveToFile")

6. Scene transition
   → Load ResultsScene
```

---

## Implementation Checklist

### Phase 1: Core Managers (Week 1)
- [ ] Create **ProjectBlastGameManager** (extends GameManager)
- [ ] Create **PlayerProgressManager** (MMPersistentSingleton + IMMPersistent)
- [ ] Create **HeroCollectionManager** (MMPersistentSingleton + IMMPersistent)
- [ ] Create **DeckManager** (MMPersistentSingleton + IMMPersistent)
- [ ] Implement save/load with MMPersistenceManager
- [ ] Test persistence across scene loads

### Phase 2: ScriptableObjects (Week 1)
- [ ] Create **HeroDataSO** template
- [ ] Create **WeaponDataSO** template
- [ ] Create **LevelDataSO** template
- [ ] Create 3-5 test heroes
- [ ] Create 3-5 test levels
- [ ] Implement stat scaling formulas

### Phase 3: Scene Structure (Week 2)
- [ ] Create **BootScene** with persistent manager initialization
- [ ] Create **MainMenuScene** with UI layout
- [ ] Create **HeroCollectionScene** with hero grid UI
- [ ] Create **DeckBuildingScene** with deck slots UI
- [ ] Create **LevelSelectScene** with level buttons
- [ ] Update **CombatScene** with WaveManager integration
- [ ] Create **ResultsScene** with rewards display

### Phase 4: UI Implementation (Week 2)
- [ ] Main Menu UI (play, heroes, deck, shop buttons)
- [ ] Hero Collection UI (grid, filters, stats, upgrade)
- [ ] Deck Builder UI (5 slots, hero selector, validation)
- [ ] Level Select UI (buttons, stars, locked/unlocked)
- [ ] Combat HUD (wave progress, currency, ammo)
- [ ] Results UI (stars, rewards, next level)
- [ ] Currency display (persistent across scenes)

### Phase 5: Gameplay Integration (Week 3)
- [ ] Create **WaveManager** (spawn system)
- [ ] Integrate **CombatCoordinator** (already complete)
- [ ] Implement hero spawning from deck
- [ ] Implement wave progression logic
- [ ] Victory/defeat conditions
- [ ] Star rating calculation
- [ ] Currency rewards
- [ ] Hero XP awards

### Phase 6: Events & Communication (Week 3)
- [ ] Define **LevelEvent** struct
- [ ] Define **HeroEvent** struct
- [ ] Define **CurrencyEvent** struct
- [ ] Define **ProgressEvent** struct
- [ ] Implement event listeners in all managers
- [ ] Implement event triggers in gameplay systems
- [ ] Test event flow (spawn → combat → victory → rewards)

### Phase 7: Polish & Testing (Week 4)
- [ ] Scene transition animations
- [ ] Sound effects (MMSoundManager)
- [ ] UI animations (DOTween)
- [ ] Save/load testing
- [ ] Currency flow testing
- [ ] Hero progression testing
- [ ] Level progression testing
- [ ] Build test APK

---

## TDE Best Practices Summary

### ✅ DO
- Use **MMPersistentSingleton** for managers that persist across scenes
- Use **MMSingleton** for scene-specific managers
- Use **MMEventManager** for decoupled communication (no direct references)
- Use **ScriptableObjects** for data (heroes, weapons, levels)
- Use **IMMPersistent** + **MMPersistenceManager** for save/load
- Use **MMSceneLoadingManager** for scene transitions with loading screens
- Use **MMSoundManager** for audio (music, SFX)
- Use **MMFeedbacks** for game feel (hit reactions, UI feedback)

### ❌ DON'T
- Don't use singleton.Instance calls across scenes (use events instead)
- Don't use FindObjectsOfType in Update() (cache or use inspector assignments)
- Don't hard-code data (use ScriptableObjects)
- Don't use PlayerPrefs for complex save data (use MMPersistenceManager)
- Don't create your own scene loading (use TDE's loading system)

### 🎯 Key Patterns
1. **Manager → Event → Listener** (not Manager → Manager)
2. **ScriptableObject → Instance Data → Runtime** (data-driven)
3. **Boot → Menu → Gameplay → Results** (clear flow)
4. **Save on key events** (level complete, quit, settings change)
5. **Load on boot** (single load point)

---

## Next Steps

1. **Start with Phase 1**: Create the 4 persistent managers
2. **Test persistence**: Boot → Menu → Combat → Save → Reload
3. **Build Phase 2**: Create ScriptableObjects for heroes/levels
4. **Implement Phase 3**: Scene structure with UI mockups
5. **Integrate Phase 5**: Combat with waves and victory conditions
6. **Polish Phase 7**: Animations, audio, feedback

---

**Reference Documentation:**
- TDE Docs: https://topdown-engine-docs.moremountains.com/
- MMTools (Singletons, Events, Save): https://feel-docs.moremountains.com/
- MMPersistenceManager Guide: https://topdown-engine-docs.moremountains.com/persistence.html

**End of Architecture Document**
