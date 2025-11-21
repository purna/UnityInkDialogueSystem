// ==========================================
// STEP-BY-STEP SCENE SETUP GUIDE
// ==========================================

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                          PART 1: CREATE MANAGERS                          ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. CREATE EMPTY GAMEOBJECTS IN YOUR SCENE:
// - Right-click in Hierarchy → Create Empty
// - Name them:
//   ✓ "SkillTreeManager"
//   ✓ "CollectableDetectionSystem"

// 2. ADD COMPONENTS:
// - Select "SkillTreeManager" → Add Component → SkillTreeManager script
// - Select "CollectableDetectionSystem" → Add Component → CollectableDetectionSystem script

// Note: InventoryManager should already exist in your scene

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                     PART 2: CREATE SKILL SCRIPTABLEOBJECTS               ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. CREATE A SKILL TREE CONTAINER:
// - Right-click in Project → Create → Skill Tree → Container
// - Name it: "MainSkillTree"

// 2. CREATE SKILL GROUPS (Optional - for organizing skills):
// - Right-click in Project → Create → Skill Tree → Group
// - Name them: "CombatSkills", "UtilitySkills", etc.

// 3. CREATE SKILLS:
// - Right-click in Project → Create → Skill Tree → Skill
// - Configure each skill in Inspector:
//   ✓ Skill Name: "Enhanced Detection"
//   ✓ Description: "Reveals nearby collectables"
//   ✓ Icon: Drag sprite here
//   ✓ Unlock Cost: 2 (skill points needed)
//   ✓ Tier: 1
//   ✓ Max Level: 3
//   ✓ Requires Special Key: Check if needs key
//   ✓ Required Key Name: "Detection Manual" (if key required)

// 4. CREATE SPECIAL SKILLS:
// - Right-click → Create → Skill Tree → Skills → Collectable Detection
// - Set Detection Radius: 15
// - Show On Minimap: True

// 5. LINK SKILLS TOGETHER:
// - Open a skill in Inspector
// - Under "Prerequisites", drag skills that must be unlocked first
// - Under "Children", drag skills that unlock after this one

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                  PART 3: CREATE COLLECTABLE SCRIPTABLEOBJECTS            ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. CREATE SKILL POINT COLLECTABLES:
// - Right-click → Create → Pixelagent → Collectable → Skill Points
// - Name: "SkillPoint_Basic"
// - Configure:
//   ✓ Item Name: "Skill Point"
//   ✓ Item Icon: Drag icon sprite
//   ✓ Skill Points Amount: 1
//   ✓ Collection Flash Time: 0.5
//   ✓ Collect Color: Yellow

// 2. CREATE SKILL TREE KEYS:
// - Right-click → Create → Pixelagent → Collectable → Skill Tree Key
// - Name: "Key_CombatBranch"
// - Configure:
//   ✓ Item Name: "Combat Manual"
//   ✓ Item Icon: Drag key sprite
//   ✓ Skill Tree Group Name: "CombatSkills" (or leave blank)
//   ✓ Target Group: Drag SkillsTreeGroup SO here

// 3. CREATE SKILL UNLOCK COLLECTABLES:
// - Right-click → Create → Pixelagent → Collectable → Skill Tree Upgrade
// - Name: "AutoUnlock_Detection"
// - Configure:
//   ✓ Skill To Unlock: Drag skill SO here
//   ✓ Skill Points To Grant: 0
//   ✓ Auto Unlock Skill: True

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                     PART 4: CREATE COLLECTABLE PREFABS                    ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. CREATE SKILL POINT PICKUP:
// - Create 2D Sprite or 3D Object in scene
// - Name it: "SkillPointPickup"
// - Add Components:
//   ✓ CircleCollider2D (set Is Trigger = true)
//   ✓ Collectable script
//   ✓ CollectableTriggerHandler script (if you have one)
// - In Collectable component:
//   ✓ Drag "SkillPoint_Basic" SO into Collectable field
// - Drag to Project to make prefab
// - Delete from scene

// 2. CREATE KEY PICKUP:
// - Same process as above
// - Name: "KeyPickup"
// - Drag "Key_CombatBranch" SO into Collectable component
// - Make prefab

// 3. CREATE ENHANCED COLLECTABLES (Requires Skills):
// - Create sprite/object
// - Add EnhancedCollectable component instead of Collectable
// - Configure:
//   ✓ Collectable: Drag collectable SO
//   ✓ Requires Skill: True
//   ✓ Required Skill: Drag skill SO that's needed
//   ✓ Sprite Renderer: Drag sprite renderer
//   ✓ Locked Color: Gray

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                        PART 5: CREATE SKILL GATED CHEST                   ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. CREATE CHEST OBJECT:
// - Create 2D Sprite
// - Name: "SkillGatedChest"
// - Add Components:
//   ✓ BoxCollider2D (Is Trigger = true)
//   ✓ SkillGatedChest script
// - Configure:
//   ✓ Required Skill: Drag skill SO
//   ✓ Collectable Rewards: Drag skill point prefabs
//   ✓ Skill Point Reward: 5
//   ✓ Sprite Renderer: Drag sprite renderer
//   ✓ Locked Sprite: Chest closed sprite
//   ✓ Unlocked Sprite: Chest open sprite

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                          PART 6: SETUP SKILL TREE UI                      ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. CREATE UI CANVAS (if not exists):
// - Right-click Hierarchy → UI → Canvas
// - Canvas Scaler: Scale With Screen Size

// 2. CREATE SKILL TREE PANEL:
// - Right-click Canvas → UI → Panel
// - Name: "SkillTreePanel"
// - Set it inactive by default (uncheck at top of Inspector)

// 3. CREATE SKILL POINTS HUD:
// - Right-click Canvas → UI → Text - TextMeshPro
// - Name: "SkillPointsText"
// - Position at top-right corner
// - Add SkillPointsHUD component
// - Drag text into Skill Points Text field

// 4. CREATE SKILL TREE UI:
// - Right-click SkillTreePanel → Create Empty
// - Name: "SkillTreeUI"
// - Add SkillTreeUIController component
// - Create child objects for UI elements:
//   ✓ Skill Node Parent (Empty with GridLayoutGroup or ScrollRect)
//   ✓ Skill Details Panel (shows selected skill info)
//   ✓ Unlock Button

// 5. CREATE SKILL NODE PREFAB:
// - Right-click SkillTreePanel → UI → Button
// - Name: "SkillNodePrefab"
// - Add SkillNodeUI component
// - Add these child objects:
//   ✓ Icon (Image)
//   ✓ Background (Image)
//   ✓ SkillName (TextMeshPro)
//   ✓ Cost (TextMeshPro)
//   ✓ Level (TextMeshPro)
//   ✓ LockedOverlay (Image)
//   ✓ KeyRequiredIcon (Image)
// - Drag these into SkillNodeUI component fields
// - Drag to Project folder to make prefab
// - Delete from scene

// 6. SETUP SKILL TREE UI CONTROLLER:
// - Select SkillTreeUI object
// - In SkillTreeUIController component:
//   ✓ Skill Node Prefab: Drag SkillNodePrefab
//   ✓ Skill Node Parent: Drag parent container
//   ✓ Skill Points Text: Drag text object
//   ✓ Skill Name Text: Drag text in details panel
//   ✓ Skill Description Text: Drag text in details panel
//   ✓ Skill Icon Image: Drag image in details panel
//   ✓ Unlock Button: Drag button
//   ✓ Set colors for locked/unlocked/available

// 7. CREATE PANEL CONTROLLER:
// - Create empty GameObject in Canvas
// - Name: "SkillTreePanelController"
// - Add SkillTreePanelController component
// - Configure:
//   ✓ Skill Tree Panel: Drag SkillTreePanel
//   ✓ Skill Tree UI: Drag SkillTreeUI object
//   ✓ Toggle Key: K (or your preference)
//   ✓ Pause Game When Open: True

// 8. CREATE NOTIFICATION UI (Optional):
// - Right-click Canvas → UI → Panel
// - Name: "SkillNotificationPanel"
// - Position at bottom-center
// - Add SkillNotificationUI component
// - Add TextMeshPro and Image children
// - Link to component fields

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                      PART 7: CONFIGURE MANAGERS IN SCENE                  ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. SETUP SKILLTREEMANAGER:
// - Select SkillTreeManager object
// - In SkillTreeManager component:
//   ✓ Skill Tree Container: Drag "MainSkillTree" SO
//   ✓ Current Skill Points: 3 (starting points)

// 2. SETUP COLLECTABLE DETECTION SYSTEM:
// - Select CollectableDetectionSystem object
// - Create a simple sprite/prefab for indicator
// - Drag into Detection Indicator Prefab field
// - Update Interval: 0.5

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                        PART 8: PLACE COLLECTABLES IN SCENE               ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// 1. DRAG PREFABS INTO SCENE:
// - Drag SkillPointPickup prefabs around the level
// - Drag KeyPickup prefabs in special locations
// - Place SkillGatedChest where skills are required

// 2. TEST PLAYER INTERACTION:
// - Player should have CollectableTriggerHandler or similar
// - When player touches collectable, it calls Collect()

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                            PART 9: TESTING CHECKLIST                      ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// ✓ Player can collect skill points
// ✓ Skill points display updates
// ✓ Press K to open skill tree
// ✓ Skills show locked/unlocked states
// ✓ Can unlock skills with points
// ✓ Prerequisites work correctly
// ✓ Keys can be collected
// ✓ Skills requiring keys show indicator
// ✓ Enhanced collectables require correct skill
// ✓ Chests require correct skill to open
// ✓ Detection skill reveals nearby items

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                          EXAMPLE SKILL TREE SETUP                         ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// EXAMPLE SKILL PROGRESSION:

// Tier 1 (Starting Skills - No prerequisites):
// - "Keen Eye" (Passive): +10% item detection range | Cost: 1 SP
// - "Quick Hands" (Passive): Collect items faster | Cost: 1 SP

// Tier 2 (Requires Tier 1):
// - "Treasure Hunter" (Active): Reveals nearby collectables | Cost: 2 SP
//   Prerequisites: Keen Eye
//   Special: This should be CollectableDetectionSkill type

// - "Lucky Find" (Passive): +15% rare item drop rate | Cost: 2 SP
//   Prerequisites: Quick Hands

// Tier 3 (Requires Tier 2 + Key):
// - "Master Collector" (Passive): Double skill points from pickups | Cost: 3 SP
//   Prerequisites: Treasure Hunter, Lucky Find
//   Requires Special Key: "Collector's License"

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                         COMMON ISSUES & SOLUTIONS                         ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// ISSUE: "SkillTreeManager.Instance is null"
// SOLUTION: Make sure SkillTreeManager GameObject is active in scene

// ISSUE: "Cannot add item to inventory"
// SOLUTION: Check InventoryManager is in scene and has been updated

// ISSUE: "Skill won't unlock"
// SOLUTION: Check prerequisites are met, enough points, and key if needed

// ISSUE: "Detection skill doesn't work"
// SOLUTION: Make sure skill type is CollectableDetectionSkill, not base Skill

// ISSUE: "UI doesn't show"
// SOLUTION: Check SkillTreePanel is child of Canvas, check EventSystem exists

// ISSUE: "Collectables don't trigger"
// SOLUTION: Check CircleCollider2D Is Trigger is enabled, Player has correct tag

/* 
╔══════════════════════════════════════════════════════════════════════════╗
║                          QUICK START EXAMPLE SCENE                        ║
╚══════════════════════════════════════════════════════════════════════════╝
*/

// MINIMAL WORKING SETUP:

/*
Hierarchy:
├── Canvas
│   ├── SkillPointsText (SkillPointsHUD)
│   └── SkillTreePanel (inactive)
│       └── SkillTreeUI (SkillTreeUIController)
├── Managers
│   ├── SkillTreeManager (SkillTreeManager script)
│   ├── InventoryManager (Already exists)
│   └── CollectableDetectionSystem
├── Player
│   └── (Your player with tag "Player")
└── World
    ├── SkillPointPickup (x10)
    └── KeyPickup (x2)

Project Assets:
├── ScriptableObjects/
│   ├── SkillTree/
│   │   ├── MainSkillTree (SkillsTreeContainer)
│   │   ├── Skills/
│   │   │   ├── Skill_KeenEye (Skill)
│   │   │   └── Skill_TreasureHunter (CollectableDetectionSkill)
│   │   └── Groups/ (optional)
│   └── Collectables/
│       ├── SkillPoint_Basic (CollectableSkillPointsSO)
│       └── Key_DetectionManual (CollectableSkillTreeKeySO)
└── Prefabs/
    ├── SkillPointPickup
    └── KeyPickup
*/