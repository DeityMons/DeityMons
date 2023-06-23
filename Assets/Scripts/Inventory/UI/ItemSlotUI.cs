using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text countText;
    [SerializeField] Image itemIcon;
    [SerializeField] Image Cover;

    RectTransform rectTransform;
    private void Awake()
    {

    }

    public Text NameText => nameText;
    public Text CountText => countText;
    public Image Blocker => Cover;

    public float Height => rectTransform.rect.height;

    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
        itemIcon.sprite = itemSlot.Item.Icon;
        GetComponent<Image>().color = new Color(0.9568628f, 0.9529412f, 0.9882354f);
        Cover.color = new Color(0.8980392f, 0.8980392f, 0.8980392f);
        rectTransform = GetComponent<RectTransform>();
    }
}

