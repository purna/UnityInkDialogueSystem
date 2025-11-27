using UnityEngine;
using Core.Game;

[CreateAssetMenu(menuName = "Pixelagent/Collectable/Collectable Bonus", fileName = "Collectable Bonus Function")]
public class CollectableBonusFunction : SkillFunction
{
    [Header("Bonus Settings")]
    [SerializeField] private CollectableBonusType bonusType;
    [SerializeField] private float bonusAmount = 1.5f;

    public enum CollectableBonusType
    {
        CurrencyMultiplier,
        DropRateIncrease,
        DetectionRadius,
        CollectionSpeed
    }

    public override void Execute(Skill skill)
    {
        float value = skill.GetScaledValue();
        
        switch (bonusType)
        {
            case CollectableBonusType.CurrencyMultiplier:
                // Apply currency multiplier
                Debug.Log($"Currency collection increased by {value}x");
                break;
                
            case CollectableBonusType.DropRateIncrease:
                // Apply drop rate increase
                Debug.Log($"Drop rate increased by {value}%");
                break;
                
            case CollectableBonusType.DetectionRadius:
                // Increase detection radius
                if (CollectableDetectionSystem.Instance != null)
                {
                    CollectableDetectionSystem.Instance.EnableDetection(value, true);
                }
                break;
                
            case CollectableBonusType.CollectionSpeed:
                // Increase collection speed
                Debug.Log($"Collection speed increased by {value}x");
                break;
        }
    }
}