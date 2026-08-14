using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class WorldInitializer : MonoBehaviour
    {
        public ChunkManager chunkManager;
        private bool isSaving = false;
        public float autoSaveInterval = 10f;

        void Start()
        {
            Debug.Log(Application.persistentDataPath);
            if (SaveManager.Instance.HasSavedWorld())
            {
                SaveManager.Instance.LoadWorld();
                LoadPlacedBlocks();
            }
            StartCoroutine(AutoSaveRoutine());
        }

        public HashSet<Vector3> instantiatedPositions = new HashSet<Vector3>();
        private Dictionary<Vector3, GameObject> loadedBlocks = new Dictionary<Vector3, GameObject>();

        void LoadPlacedBlocks()
        {
            loadedBlocks.Clear();

            foreach (var pair in SaveManager.Instance.loadedBlocks)
            {
                Vector3 pos = pair.Key;

                if (!instantiatedPositions.Contains(pos))
                {
                    GameObject prefab = BlockDatabase.Instance.GetPrefab(pair.Value.blockID);
                    instantiatedPositions.Add(pos);
                    if (prefab != null)
                    {
                        GameObject block = Instantiate(prefab, pos, Quaternion.identity);
                        loadedBlocks[pos] = block;
                    }
                }
            }

            ApplyFaceCullingToLoadedBlocks();
        }

        void ApplyFaceCullingToLoadedBlocks()
        {
            foreach (var kvp in loadedBlocks)
            {
                Vector3 pos = kvp.Key;
                GameObject block = kvp.Value;

                if (block == null) continue;

                MeshRenderer[] renderers = block.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer renderer in renderers)
                {
                    bool hasTop = loadedBlocks.ContainsKey(pos + Vector3.up) || SaveManager.Instance.loadedBlocks.ContainsKey(pos + Vector3.up);
                    bool hasBottom = loadedBlocks.ContainsKey(pos + Vector3.down) || SaveManager.Instance.loadedBlocks.ContainsKey(pos + Vector3.down);
                    bool hasRight = loadedBlocks.ContainsKey(pos + Vector3.right) || SaveManager.Instance.loadedBlocks.ContainsKey(pos + Vector3.right);
                    bool hasLeft = loadedBlocks.ContainsKey(pos + Vector3.left) || SaveManager.Instance.loadedBlocks.ContainsKey(pos + Vector3.left);
                    bool hasForward = loadedBlocks.ContainsKey(pos + Vector3.forward) || SaveManager.Instance.loadedBlocks.ContainsKey(pos + Vector3.forward);
                    bool hasBack = loadedBlocks.ContainsKey(pos + Vector3.back) || SaveManager.Instance.loadedBlocks.ContainsKey(pos + Vector3.back);

                    if (hasTop && hasBottom && hasRight && hasLeft && hasForward && hasBack)
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }

        System.Collections.IEnumerator AutoSaveRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveInterval);

                if (!isSaving)
                {
                    isSaving = true;
                    _ = SaveWorldAsync();
                }
            }
        }

        private async Task SaveWorldAsync()
        {
            Debug.Log("Async saving started...");
            await Task.Run(() =>
            {
                SaveManager.Instance.SaveWorld();
            });
            Debug.Log("Async saving completed.");
            isSaving = false;
        }
    }
}
