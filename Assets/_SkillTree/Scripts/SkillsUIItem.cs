using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillsUIItem : MonoBehaviour
{
    private SkillsUI parent;
    private int index;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image iconImage;

    public void Initialize(SkillsUI parent, ScriptableObject skill, int count, int index)
    {
        this.parent = parent;
        this.index = index;

        if (skill is CollectableSOBase col)
        {
            if (nameText) nameText.text = col.ItemName;
            if (iconImage && col.ItemIcon) iconImage.sprite = col.ItemIcon;
            if (countText) countText.text = count > 1 ? "x" + count : "";
        }

        // Click to select
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => parent.SelectSkill(index));
        }
    }
}
