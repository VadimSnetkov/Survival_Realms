using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class UI_CraftingButton : MonoBehaviour
    {
        public CraftingRecipeSO recipe;

        public void OnClickCraft()
        {
            CraftingManager.Instance.CraftItem(recipe);
        }
    }
}
