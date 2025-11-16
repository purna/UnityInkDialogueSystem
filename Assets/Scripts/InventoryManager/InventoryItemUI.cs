using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Core.Game;

public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text nameText;

    [SerializeField] private TMP_Text descriptionText;

    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image iconImage;

    private CollectableUpgradeSO itemData;
    private InventoryUI inventoryUI;
    private int itemIndex;


    public void Initialize(InventoryUI ui, CollectableUpgradeSO item, int count, int index)
    {
        inventoryUI = ui;
        itemData = item;
        itemIndex = index;

        
        if (nameText != null)
            nameText.text = item.ItemName;

        if (descriptionText != null)
            descriptionText.text = item.ItemDescription;
        
        if (countText != null)
            countText.text = count > 1 ? count.ToString() : "";

        if (iconImage != null)
            iconImage.sprite = item.ItemIcon;

        // Hook up button click
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        inventoryUI.SelectItem(itemIndex);
    }

    public void OnButtonClick()
    {
        inventoryUI.SelectItem(itemIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show item details when the pointer enters the item UI
        Debug.Log($"Pointer entered item: {itemData.ItemName}");

        inventoryUI.ShowItemDetails(itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        int selectedIndex = inventoryUI.GetSelectedItemIndex();
        if (selectedIndex >= 0)
        {
            inventoryUI.SelectItem(selectedIndex);
        }
    }



}
