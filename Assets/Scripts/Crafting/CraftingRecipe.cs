using System.Collections.Generic;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class CraftingRecipe
    {
        public string outputItemID;
        public int outputAmount;
        public List<CraftingIngredient> ingredients;
    }

    [System.Serializable]
    public class CraftingIngredient
    {
        public string itemID;
        public int amount;
    }
}
