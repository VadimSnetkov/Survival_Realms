using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class ChunkManager : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public int chunkSize = 16;
        public int viewDistance = 1;


        public VoxelWorldGenerator generatorReference;

        private Transform player;
        private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();
        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            StartCoroutine(PlacePlayerOnSurface());

        }

        private bool isUpdatingChunks = false;
        private Vector2Int lastPlayerChunk;
        private float chunkUpdateTimer;
        public float chunkUpdateInterval = 0.25f;

        private void Update()
        {
            chunkUpdateTimer += Time.deltaTime;
            if (chunkUpdateTimer < chunkUpdateInterval) return;
            chunkUpdateTimer = 0f;

            Vector2Int currentChunk = new Vector2Int(
                Mathf.FloorToInt(player.position.x / chunkSize),
                Mathf.FloorToInt(player.position.z / chunkSize)
            );

            if (currentChunk != lastPlayerChunk && !isUpdatingChunks)
            {
                lastPlayerChunk = currentChunk;
                StartCoroutine(UpdateChunksAsync());
            }
        }

        IEnumerator PlacePlayerOnSurface()
        {
            yield return null;

            int x = Mathf.RoundToInt(player.position.x);
            int z = Mathf.RoundToInt(player.position.z);

            int surfaceY = generatorReference.GetSurfaceY(x, z);

            player.position = new Vector3(x + 0.5f, surfaceY + 50f, z + 0.5f);
        }

        IEnumerator UpdateChunksAsync()
        {
            isUpdatingChunks = true;

            Vector2Int playerChunkCoord = new Vector2Int(
                Mathf.FloorToInt(player.position.x / chunkSize),
                Mathf.FloorToInt(player.position.z / chunkSize)
            );

            HashSet<Vector2Int> neededChunks = new();

            for (int dx = -viewDistance; dx <= viewDistance; dx++)
            {
                for (int dz = -viewDistance; dz <= viewDistance; dz++)
                {
                    Vector2Int coord = new Vector2Int(playerChunkCoord.x + dx, playerChunkCoord.y + dz);
                    neededChunks.Add(coord);

                    if (!loadedChunks.ContainsKey(coord))
                    {
                        Vector3 chunkPos = new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize);
                        GameObject newChunk = Instantiate(chunkPrefab, chunkPos, Quaternion.identity, transform);
                        newChunk.name = $"Chunk_{coord.x}_{coord.y}";

                        bool chunkHasSavedData = false;
                        if (SaveManager.Instance != null)
                        {
                            foreach (var kvp in SaveManager.Instance.loadedBlocks)
                            {
                                Vector3 blockPos = kvp.Key;
                                if (Mathf.FloorToInt(blockPos.x / chunkSize) == coord.x &&
                                    Mathf.FloorToInt(blockPos.z / chunkSize) == coord.y)
                                {
                                    chunkHasSavedData = true;
                                    break;
                                }
                            }
                        }

                        if (!chunkHasSavedData)
                        {
                            generatorReference.GenerateChunk(newChunk.transform, chunkSize);
                        }

                        loadedChunks.Add(coord, new Chunk
                        {
                            chunkCoord = coord,
                            chunkObject = newChunk,
                            isActive = true
                        });

                        yield return null;
                    }

                    else
                    {
                        loadedChunks[coord].chunkObject.SetActive(true);
                        loadedChunks[coord].isActive = true;
                    }
                }
            }

            foreach (var kvp in loadedChunks)
            {
                if (!neededChunks.Contains(kvp.Key))
                {
                    kvp.Value.chunkObject.SetActive(false);
                    kvp.Value.isActive = false;
                }
            }

            isUpdatingChunks = false;
        }

    }
}