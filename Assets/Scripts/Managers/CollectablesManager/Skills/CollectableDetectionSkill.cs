// ═══════════════════════════════════════════════════════════════════════════
// SKILL: COLLECTABLE DETECTION
// ═══════════════════════════════════════════════════════════════════════════
using UnityEngine;
using Core.Game;

/// <summary>
/// Collectable Detection Skill - Reveals nearby collectables to the player
/// 
/// PURPOSE:
/// - Helps players find hidden collectables within a radius
/// - Can show collectables on minimap for easier navigation
/// - Controlled by CollectableDetectionSystem
/// 
/// WHEN TO USE:
/// - As an unlockable skill in the skill tree
/// - Explorer/treasure hunter character builds
/// - Mid-game progression to help find secrets
/// - Quality of life improvement for completionists
/// 
/// CONFIGURATION:
/// - detectionRadius: How far around the player to detect collectables (default: 15 units)
/// - showOnMinimap: Whether to display detected collectables on the minimap
/// 
/// EXAMPLE SETUPS:
/// - Basic Detection: radius = 10, minimap = false (simple proximity indicator)
/// - Advanced Detection: radius = 20, minimap = true (full minimap integration)
/// - Treasure Hunter: radius = 30, minimap = true (wide area + map markers)
/// 
/// WHAT HAPPENS:
/// ON UNLOCK:
/// 1. Activates CollectableDetectionSystem
/// 2. Sets detection radius and minimap settings
/// 3. Player can now see nearby collectables
/// 
/// ON RESET/LOCK:
/// 1. Disables detection system
/// 2. Hides all collectable indicators
/// 3. Minimap markers removed
/// 
/// INTEGRATION:
/// - Requires CollectableDetectionSystem singleton in the scene
/// - Works with any collectable type (currency, skill points, items, etc.)
/// - Can be upgraded by creating multiple versions with different radii
/// </summary>
[CreateAssetMenu(fileName = "Collectable Detection Skill", menuName = "Pixelagent/Collectable/Collectable Detection Skill")]
public class CollectableDetectionSkill : Skill
{
    [Header("Detection Settings")]
    [Tooltip("Radius around player to detect collectables (in units)")]
    [SerializeField] private float detectionRadius = 15f;
    
    [Tooltip("Show detected collectables on the minimap")]
    [SerializeField] private bool showOnMinimap = true;

    public float DetectionRadius => detectionRadius;
    public bool ShowOnMinimap => showOnMinimap;

    // Override Unlock to add detection behavior
    public new void Unlock()
    {
        base.Unlock();
        
        // Enable collectable detection in the game
        if (CollectableDetectionSystem.Instance != null)
        {
            CollectableDetectionSystem.Instance.EnableDetection(detectionRadius, showOnMinimap);
        }
    }

    // Override Reset to disable detection
    public new void Reset()
    {
        base.Reset();
        
        // Disable detection when skill is locked/reset
        if (CollectableDetectionSystem.Instance != null)
        {
            CollectableDetectionSystem.Instance.DisableDetection();
        }
    }
}