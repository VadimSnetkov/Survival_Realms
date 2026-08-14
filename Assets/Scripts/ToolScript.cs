using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class ToolScript : MonoBehaviour
    {
        public int Health = 100;
        public int Damage = 1;
        public string Name = "";

        public void TakeDamage()
        {
            Health -= 1;
            if (Health <= 0)
            {
                InventoryManager.Instance.RemoveItem(Name, 1);
                Destroy(gameObject);
            }
        }
    }
}
