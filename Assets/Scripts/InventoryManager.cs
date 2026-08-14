using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

namespace BlockBuildingCraftingSystem
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;
        public InputAction[] hotbarNumberActions;
        [System.Serializable]
        public class InventoryItem
        {
            public string id;
            public int quantity;
        }

        [System.Serializable]
        public class InventorySaveData
        {
            public List<InventoryItem> items;
        }

        public List<InventoryItem> items = new List<InventoryItem>();
        public int selectedIndex = 0;

        private string inventoryPath;
        public InputAction mouseScrollAction;
        public InputAction shoulderLeftAction;
        public InputAction shoulderRightAction;

        public void SelectItem(int index)
        {
            if (index >= 0 && index < items.Count)
            {
                selectedIndex = index;
                UI_Hotbar.Instance.UpdateUI();

                var selectedItem = GetSelectedHotbarItem();
                if (selectedItem != null)
                {
                    var blockData = BlockDatabase.Instance.blocks
                        .Find(b => b.id == selectedItem.id);

                    if (blockData != null && blockData.type == BlockType.Item)
                    {
                        FPSHandRotator.Instance.Switch_Hand();
                    }
                    else
                    {
                        FPSHandRotator.Instance.Clear_Hand();
                    }
                }
                else
                {
                    FPSHandRotator.Instance.Clear_Hand();
                }
            }
        }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            inventoryPath = Application.persistentDataPath + "/inventory_save.json";
            
            mouseScrollAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/scroll");
            mouseScrollAction.Enable();

            shoulderLeftAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/leftShoulder");
            shoulderLeftAction.Enable();

            shoulderRightAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/rightShoulder");
            shoulderRightAction.Enable();
            int count = 9;
            hotbarNumberActions = new InputAction[count];

            for (int i = 0; i < count; i++)
            {
                hotbarNumberActions[i] = new InputAction(
                    name: $"Hotbar_{i + 1}",
                    type: InputActionType.Button,
                    binding: $"<Keyboard>/{i + 1}"
                );
                hotbarNumberActions[i].Enable();
            }
        }

        void Start()
        {
            LoadInventory();
            if (items.Count > 0)
            {
                SelectItem(selectedIndex);
            }
        }

        public void AddItem(string id, int amount)
        {
            InventoryItem item = items.Find(i => i.id == id);
            if (item == null)
            {
                item = new InventoryItem { id = id, quantity = 0 };
                items.Add(item);
            }

            item.quantity += amount;

            SaveInventory();
            UI_Hotbar.Instance.UpdateUI();
        }

        public InventoryItem GetSelectedHotbarItem()
        {
            if (items.Count == 0) return null;
            return items[selectedIndex % items.Count];
        }


        public string GetSelectedBlockID()
        {
            if (items.Count == 0) return null;
            return items[selectedIndex % items.Count].id;
        }

        public GameObject GetSelectedBlockPrefab()
        {
            string id = GetSelectedBlockID();
            return BlockDatabase.Instance.GetPrefab(id);
        }

        void Update()
        {
            if (items.Count > 0 && hotbarNumberActions != null)
            {
                for (int i = 0; i < hotbarNumberActions.Length; i++)
                {
                    if (hotbarNumberActions[i].WasPressedThisFrame())
                    {
                        SelectItem(i);
                        break;
                    }
                }
            }
            if (GameCanvas.Instance.isPaused) return;
            if (CraftingManager.Instance.panelCrafting.activeSelf) return;

            Vector2 scrollValue = mouseScrollAction.ReadValue<Vector2>();
            float scroll = scrollValue.y;

            if (scroll > 0f)
                ChangeSelection(1);
            else if (scroll < 0f)
                ChangeSelection(-1);

            if (shoulderRightAction.WasPressedThisFrame())
                ChangeSelection(1);

            if (shoulderLeftAction.WasPressedThisFrame())
                ChangeSelection(-1);
        }

        public void ChangeSelection(int delta)
        {
            if (items.Count == 0) return;
            selectedIndex = (selectedIndex + delta + items.Count) % items.Count;
            UI_Hotbar.Instance.UpdateUI();

            var selectedItem = GetSelectedHotbarItem();
            if (selectedItem != null)
            {
                var blockData = BlockDatabase.Instance.blocks
                    .Find(b => b.id == selectedItem.id);

                if (blockData != null && blockData.type == BlockType.Item)
                {
                    FPSHandRotator.Instance.Switch_Hand();
                }
                else
                {
                    FPSHandRotator.Instance.Clear_Hand();
                }
            }
            else
            {
                FPSHandRotator.Instance.Clear_Hand();
            }
        }

        public void SaveInventory()
        {
            InventorySaveData data = new InventorySaveData
            {
                items = items
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(inventoryPath, json);
            Debug.Log("Inventory saved.");
        }

        public bool HasItem(string id)
        {
            InventoryItem item = items.Find(i => i.id == id);
            return item != null && item.quantity > 0;
        }

        public void RemoveItem(string id, int amount)
        {
            InventoryItem item = items.Find(i => i.id == id);
            if (item == null) return;

            item.quantity -= amount;
            if (item.quantity <= 0)
            {
                items.Remove(item);
                selectedIndex = Mathf.Clamp(selectedIndex, 0, items.Count - 1);
            }

            SaveInventory();
            UI_Hotbar.Instance.UpdateUI();
        }

        public void LoadInventory()
        {
            if (!File.Exists(inventoryPath))
            {
                Debug.Log("No inventory save found.");
                return;
            }

            string json = File.ReadAllText(inventoryPath);
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);

            items = data.items ?? new List<InventoryItem>();
            UI_Hotbar.Instance.UpdateUI();
            Debug.Log("Inventory loaded. " + items.Count + " item(s).");
        }
    }
}
