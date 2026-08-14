using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;


namespace BlockBuildingCraftingSystem
{
    [System.Serializable]
    public class SavedBlock
    {
        public string blockID;
        public Vector3 position;
    }

    [System.Serializable]
    public class BlockSaveData
    {
        public List<SavedBlock> placedBlocks = new List<SavedBlock>();
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance;

        private string savePath;
        public string mapName = "world";

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            savePath = Application.persistentDataPath + "/" + mapName + ".json";
        }

        private BlockSaveData saveData = new BlockSaveData();

        public void RegisterBlock(string id, Vector3 pos)
        {
            saveData.placedBlocks.Add(new SavedBlock
            {
                blockID = id,
                position = pos
            });
        }

        IEnumerator AutoSaveRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);
                SaveWorld();
            }
        }

        public void UnregisterBlock(Vector3 pos)
        {
            for (int i = 0; i < saveData.placedBlocks.Count; i++)
            {
                if (saveData.placedBlocks[i].position == pos)
                {
                    saveData.placedBlocks.RemoveAt(i);
                    break;
                }
            }
        }

        [System.Serializable]
        public class BlockSaveData
        {
            public List<SavedBlock> placedBlocks = new List<SavedBlock>();
            public List<Vector3> destroyedBlocks = new List<Vector3>();
        }

        public bool IsBlockDestroyed(Vector3 pos)
        {
            return saveData.destroyedBlocks.Contains(pos);
        }

        public void RegisterDestroyedBlock(Vector3 pos)
        {
            if (!saveData.destroyedBlocks.Contains(pos))
                saveData.destroyedBlocks.Add(pos);
        }

        public bool HasBlock(Vector3 pos)
        {
            return loadedBlocks.ContainsKey(pos);
        }

        public bool HasSavedWorld()
        {
            Debug.Log("HasSavedWorld check: " + savePath);

            if (!File.Exists(savePath))
                return false;

            string json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<BlockSaveData>(json);

            return data != null && data.placedBlocks != null && data.placedBlocks.Count > 0;
        }


        public void SaveWorld()
        {
            List<SavedBlock> allBlocks = new List<SavedBlock>();

            foreach (var bh in FindObjectsByType<BlockHealth>(FindObjectsSortMode.None))
            {
                allBlocks.Add(new SavedBlock
                {
                    blockID = bh.blockID,
                    position = bh.transform.position
                });
            }

            saveData.placedBlocks = allBlocks;

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Saved to: " + savePath);
        }

        private string GetSavePath()
        {
            return savePath;
        }

        bool isLoaded = false;
        public Dictionary<Vector3, SavedBlock> loadedBlocks = new Dictionary<Vector3, SavedBlock>();

        public void LoadWorld()
        {
            if (!File.Exists(savePath)) return;
            if (isLoaded) return;

            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<BlockSaveData>(json);

            loadedBlocks.Clear();
            foreach (var block in saveData.placedBlocks)
            {
                loadedBlocks[block.position] = block;
            }

            isLoaded = true;
            Debug.Log("World loaded into memory.");
        }

        public void DeleteSaveFile()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Deleted save: " + savePath);
            }

            isLoaded = false;
            loadedBlocks.Clear();
            saveData = new BlockSaveData();
        }
    }
}
