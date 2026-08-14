using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace BlockBuildingCraftingSystem
{
    public class UI_Hotbar : MonoBehaviour
    {
        public static UI_Hotbar Instance;

        public Transform slotParent;
        public GameObject slotPrefab;

        private List<GameObject> slots = new List<GameObject>();

        void Awake()
        {
            Instance = this;
        }


        public void UpdateUI()
        {
            foreach (Transform child in slotParent)
                Destroy(child.gameObject);

            slots.Clear();

            var items = InventoryManager.Instance.items;

            for (int i = 0; i < items.Count; i++)
            {
                int index = i;
                GameObject slot = Instantiate(slotPrefab, slotParent);

                Text label = slot.GetComponentInChildren<Text>();
                label.text = $"{items[i].id} ({items[i].quantity})";

                Image[] images = slot.GetComponentsInChildren<Image>();
                foreach (Image img in images)
                {
                    if (img.gameObject.name == "Image")
                    {
                        Sprite icon = BlockDatabase.Instance.GetIcon(items[i].id);
                        img.sprite = icon;
                        break;
                    }
                }

                Button button = slot.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        InventoryManager.Instance.SelectItem(index);
                        UpdateUI();
                    });
                }

                slot.GetComponent<Image>().color =
                    (i == InventoryManager.Instance.selectedIndex) ? Color.yellow : Color.white;

                slots.Add(slot);
            }
        }

    }
}