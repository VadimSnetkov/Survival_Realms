using System.Collections;
using System.Linq;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class FPSHandRotator : MonoBehaviour
    {
        public float speed = 2.5f;
        public static FPSHandRotator Instance;

        public GameObject Hands_Parent;
        public Animator animationHand;
        public Transform itemPoint;
        [HideInInspector]
        public ToolScript currentItem;

        private void Awake()
        {
            Instance = this;
        }

        public void Hide_HandParent()
        {
            Destroy(currentItem.gameObject);
            Hands_Parent.SetActive(false);
        }

        public void Show_HandParent()
        {
            Hands_Parent.SetActive(true);
        }

        public void Clear_Hand()
        {
            if (currentItem != null)
                Destroy(currentItem.gameObject);
        }

        public void Switch_Hand()
        {
            if(currentItem != null)
            Destroy(currentItem.gameObject);

            var selectedItem = InventoryManager.Instance.GetSelectedHotbarItem();
            if (selectedItem != null)
            {
                var blockData = BlockDatabase.Instance.blocks
                    .FirstOrDefault(b => b.id == selectedItem.id);

                if (blockData != null && blockData.type == BlockType.Item)
                {
                    var tool = Instantiate(blockData.prefab, itemPoint.position, itemPoint.rotation, itemPoint.parent);
                    currentItem = tool.GetComponent<ToolScript>();
                    return;
                }
            }
        }
    }
}
