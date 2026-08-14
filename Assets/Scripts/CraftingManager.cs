using BlockBuildingCraftingSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace BlockBuildingCraftingSystem
{
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance;
        public List<CraftingRecipeSO> availableRecipes;
        public GameObject panelCrafting;
        public Transform contentListParent;
        public GameObject recipeItemPrefab;
        private CraftingRecipeSO selectedRecipe;
        [SerializeField] private List<RecipeSlot> inputSlots;
        [SerializeField] private Image resultSlot;

        private InputAction craftAction;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            craftAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/c");
            craftAction.Enable();
        }

        public void HideCraftingPanel()
        {
            panelCrafting.SetActive(false);
            HeroPlayerScript.Instance.ActivatePlayer();
            CameraScript.Instance.enabled = true;
            if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.PC)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void CraftButtonPressed()
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("No recipe selected!");
                return;
            }

            foreach (var ingredient in selectedRecipe.ingredients)
            {
                var playerHasItem = InventoryManager.Instance.items.Find(i => i.id == ingredient.id);
                if (playerHasItem == null || playerHasItem.quantity < ingredient.amount)
                {
                    Debug.LogWarning("Not enough materials to craft: " + ingredient.id);
                    return;
                }
            }

            foreach (var ingredient in selectedRecipe.ingredients)
            {
                InventoryManager.Instance.RemoveItem(ingredient.id, ingredient.amount);
            }

            InventoryManager.Instance.AddItem(selectedRecipe.outputID, 1);

            for (int i = 0; i < inputSlots.Count; i++)
            {
                inputSlots[i].text_amount.text = "";
                inputSlots[i].gameObject.SetActive(false);
            }

            resultSlot.sprite = null;

            HideCraftingPanel();

            Debug.Log("Crafted: " + selectedRecipe.outputID);
        }


        public void ShowCraftingPanel()
        {
            panelCrafting.SetActive(true);
            HeroPlayerScript.Instance.DeactivatePlayer();
            CameraScript.Instance.enabled = false;

            foreach (Transform child in contentListParent)
            {
                Destroy(child.gameObject);
            }

            foreach (CraftingRecipeSO recipe in availableRecipes)
            {
                GameObject recipeGO = Instantiate(recipeItemPrefab, contentListParent);

                Text label = recipeGO.transform.Find("Text_Name")?.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = recipe.outputID;

                Image iconImage = recipeGO.transform.Find("Image_Icon")?.GetComponent<Image>();
                if (iconImage != null)
                {
                    Sprite iconSprite = recipe.icon;
                    if (iconSprite != null)
                    {
                        iconImage.sprite = iconSprite;
                    }
                    else
                    {
                        iconImage.sprite = null;
                    }
                }

                Button btn = recipeGO.GetComponent<Button>();
                if (btn != null)
                {
                    CraftingRecipeSO localRecipe = recipe;
                    btn.onClick.AddListener(() => OnRecipeSelected(localRecipe));
                }
            }
            if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.PC)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }

        public void OnRecipeSelected(CraftingRecipeSO recipe)
        {
            selectedRecipe = recipe;

            for (int i = 0; i < inputSlots.Count; i++)
            {
                inputSlots[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < recipe.ingredients.Count && i < inputSlots.Count; i++)
            {
                var ingredient = recipe.ingredients[i];
                Sprite icon = BlockDatabase.Instance.GetIcon(ingredient.id);

                if (icon != null)
                {
                    inputSlots[i].gameObject.SetActive(true);
                    inputSlots[i].image_icon.sprite = icon;
                }

                inputSlots[i].text_amount.text = ingredient.amount.ToString();
            }

            Sprite resultIcon = recipe.icon;
            if (resultIcon != null)
            {
                resultSlot.sprite = resultIcon;
            }
            else
            {
                resultSlot.sprite = null;
            }
        }



        public bool CanCraft(CraftingRecipeSO recipe)
        {
            foreach (var ing in recipe.ingredients)
            {
                InventoryManager.InventoryItem item = InventoryManager.Instance.items.Find(i => i.id == ing.id);
                if (item == null || item.quantity < ing.amount)
                    return false;
            }
            return true;
        }

        private void Update()
        {
            if (craftAction.WasPressedThisFrame())
            {
                if (panelCrafting.activeSelf)
                    HideCraftingPanel();
                else
                    ShowCraftingPanel();
            }
        }

        public void CraftItem(CraftingRecipeSO recipe)
        {
            if (!CanCraft(recipe))
            {
                Debug.Log("Not enough ingredients!");
                return;
            }

            foreach (var ing in recipe.ingredients)
            {
                InventoryManager.Instance.RemoveItem(ing.id, ing.amount);
            }

            InventoryManager.Instance.AddItem(recipe.outputID, recipe.outputAmount);
            Debug.Log("Crafted: " + recipe.outputID);
        }
    }
}
