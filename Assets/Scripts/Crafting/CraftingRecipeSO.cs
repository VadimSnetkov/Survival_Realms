using System.Collections.Generic;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    [CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Crafting/Recipe")]
    public class CraftingRecipeSO : ScriptableObject
    {
        public string outputID;
        public int outputAmount = 1;
        public Sprite icon;

        [System.Serializable]
        public class Ingredient
        {
            public string id;
            public int amount;
        }

        public List<Ingredient> ingredients = new();
    }
}
