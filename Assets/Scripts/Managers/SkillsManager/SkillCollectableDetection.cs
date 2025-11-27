
// ===== 6. SKILL WITH COLLECTABLE DETECTION =====

// ============================================================================
// INSTRUCTIONS: Collectable Detection Skill
// ============================================================================
// HOW TO CREATE:
// 1. Right-click in the Project Window
// 2. Select: Create > Skill Tree > Skills > Collectable Detection
// 3. Fill out the settings:
//    - Detection Radius: How far the player can detect collectables
//    - Show On Minimap: If true, detected collectables appear on the minimap
//    - Collectable Layer: LayerMask used to locate collectables
//
// WHEN THIS SKILL IS UNLOCKED:
// ✔ Enables collectable detection through CollectableDetectionSystem
// ✔ Highlights or tracks collectables near the player
// ✔ Can optionally reveal collectables on the minimap
//
// WHEN THIS SKILL IS LOCKED:
// ✘ Detection is disabled
// ✘ Minimap and tracking indicators are removed
//
// USE CASES:
// - Treasure hunter perks
// - Scavenger detection abilities
// - Skills that locate items in a radius
//

using UnityEngine;

[CreateAssetMenu(menuName = "Skill Tree/Skills/Collectable Detection", fileName = "New Collectable Detection Skill")]
public class SkillCollectableDetection : Skill
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private bool showOnMinimap = true;
    [SerializeField] private LayerMask collectableLayer;

    public float DetectionRadius => detectionRadius;
    public bool ShowOnMinimap => showOnMinimap;

    protected override void OnUnlock()
    {
        base.OnUnlock();
        
        // Enable collectable detection in the game
        if (CollectableDetectionSystem.Instance != null)
        {
            CollectableDetectionSystem.Instance.EnableDetection(detectionRadius, showOnMinimap);
        }
    }

    protected override void OnLock()
    {
        base.OnLock();
        
        // Disable detection when skill is locked/reset
        if (CollectableDetectionSystem.Instance != null)
        {
            CollectableDetectionSystem.Instance.DisableDetection();
        }
    }
}
