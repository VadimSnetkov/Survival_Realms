using UnityEngine;
using System.Collections.Generic;

namespace BlockBuildingCraftingSystem
{
    public enum BlockType
    {
        Placeable,
        Item,
    }

    

    [System.Serializable]
    public class BlockDataEntry
    {
        public string id;
        public GameObject prefab;
        public Sprite icon;
        public BlockType type = BlockType.Placeable;
        public bool isCraftable = false;
    }

    public class BlockDatabase : MonoBehaviour
    {
        public static BlockDatabase Instance;

        public List<BlockDataEntry> blocks;

        private Dictionary<string, BlockDataEntry> blockLookup = new Dictionary<string, BlockDataEntry>();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            foreach (var block in blocks)
            {
                if (!blockLookup.ContainsKey(block.id))
                    blockLookup.Add(block.id, block);
            }
        }

        public Sprite GetIcon(string id)
        {
            return blockLookup.ContainsKey(id) ? blockLookup[id].icon : null;
        }

        public GameObject GetPrefab(string id)
        {
            return blockLookup.ContainsKey(id) ? blockLookup[id].prefab : null;
        }

        public BlockType GetBlockType(string id)
        {
            return blockLookup.ContainsKey(id) ? blockLookup[id].type : BlockType.Placeable;
        }

        public bool IsPlaceable(string id)
        {
            return GetBlockType(id) == BlockType.Placeable;
        }

        public bool IsInventoryItem(string id)
        {
            return GetBlockType(id) == BlockType.Item;
        }
    }
}
