using System.Collections.Generic;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public enum BiomeType { Forest, Desert, Snow }

    public class VoxelWorldGenerator : MonoBehaviour
    {
        public GameObject bedRockPrefab;
        public GameObject stonePrefab;
        public GameObject ironPrefab;
        public GameObject earthPrefab;
        public GameObject grassPrefab;
        public GameObject sandPrefab;
        public GameObject snowPrefab;
        public GameObject woodPrefab;
        public GameObject bushPrefab;

        public GameObject cactusPrefab;
        public GameObject lavaPrefab;

        public GameObject snowyBushPrefab; // Cube_SnowyBush
        public GameObject icePrefab;       // Cube_Ice

        public int chunkHeight = 64;
        public int dirtDepth = 4;

        public int seed = 12345;
        public bool randomizeSeedOnPlay = false;

        public int baseHeight = 18;
        public int heightAmplitude = 26;
        public float heightFrequency = 0.02f;
        [Range(1, 6)] public int heightOctaves = 4;

        public float mountainFrequency = 0.006f;
        public int mountainAmplitude = 22;
        [Range(0f, 1f)] public float mountainThreshold = 0.55f;

        public float biomeFrequency = 0.008f;
        [Range(0f, 1f)] public float desertThreshold = 0.35f;
        [Range(0f, 1f)] public float snowThreshold = 0.70f;

        [Range(0f, 1f)] public float treeChanceForest = 0.02f;
        [Range(0f, 1f)] public float bushChanceForest = 0.10f;
        [Range(0f, 1f)] public float bushChanceSnow = 0.05f;

        [Range(0f, 1f)] public float cactusChanceDesert = 0.05f;
        public int cactusMinHeight = 2;
        public int cactusMaxHeight = 4;

        [Range(0f, 1f)] public float ironChance = 0.03f;
        public int ironMinY = 6;
        public int ironMaxY = 28;
        public float oreFrequency = 0.08f;
        public float oreThreshold = 0.62f;

        public bool enableCaves = true;
        public float caveFrequency = 0.06f;
        public float caveThreshold = 0.63f;
        public int caveMaxY = 40;

        [Range(0f, 1f)] public float lavaLakeChance = 0.06f;
        public int lavaLakeMinRadius = 2;
        public int lavaLakeMaxRadius = 3;
        public int lavaLakeMinDepth = 1;
        public int lavaLakeMaxDepth = 3;
        public int lavaLakeCellSize = 24;

        [Range(0f, 1f)] public float iceLakeChance = 0.06f;
        public int iceLakeMinRadius = 2;
        public int iceLakeMaxRadius = 4;
        public int iceLakeMinDepth = 1;
        public int iceLakeMaxDepth = 2;
        public int iceLakeCellSize = 28;

        public bool useFaceCulling = true;
        public int generateDepthBelowSurface = 12;

        private readonly Dictionary<Vector3, GameObject> chunkBlocks = new Dictionary<Vector3, GameObject>();

        private void Awake()
        {
            if (randomizeSeedOnPlay) seed = Random.Range(0, 1000000);
        }

        private float FBM2D(float x, float z, float freq, int octaves)
        {
            float sum = 0f;
            float amp = 1f;
            float max = 0f;
            float f = freq;

            for (int i = 0; i < octaves; i++)
            {
                sum += Mathf.PerlinNoise(x * f, z * f) * amp;
                max += amp;
                amp *= 0.5f;
                f *= 2f;
            }
            return max > 0f ? sum / max : 0f;
        }

        private float Ridged2D(float x, float z, float freq, int octaves)
        {
            float sum = 0f;
            float amp = 1f;
            float max = 0f;
            float f = freq;

            for (int i = 0; i < octaves; i++)
            {
                float n = Mathf.PerlinNoise(x * f, z * f);
                n = 1f - Mathf.Abs(n * 2f - 1f);
                sum += n * amp;
                max += amp;
                amp *= 0.5f;
                f *= 2f;
            }
            return max > 0f ? sum / max : 0f;
        }

        private float Hash01(int x, int y, int z, int salt = 0)
        {
            unchecked
            {
                int h = seed;
                h = h * 31 + x;
                h = h * 31 + y;
                h = h * 31 + z;
                h = h * 31 + salt;

                h ^= (h >> 13);
                h *= 1274126177;
                h ^= (h >> 16);

                uint u = (uint)h;
                return (u & 0x00FFFFFF) / 16777215f;
            }
        }

        private float Noise3D(int x, int y, int z, float freq, int seedOffset)
        {
            float nx = (x + seed + seedOffset) * freq;
            float ny = (y + seed + seedOffset) * freq;
            float nz = (z + seed + seedOffset) * freq;
            float a = Mathf.PerlinNoise(nx, nz);
            float b = Mathf.PerlinNoise(ny, nz);
            return a * 0.6f + b * 0.4f;
        }

        private BiomeType GetBiome(int worldX, int worldZ)
        {
            float nx = (worldX + seed * 3) * biomeFrequency;
            float nz = (worldZ + seed * 3) * biomeFrequency;
            float b = Mathf.PerlinNoise(nx, nz);
            if (b < desertThreshold) return BiomeType.Desert;
            if (b > snowThreshold) return BiomeType.Snow;
            return BiomeType.Forest;
        }

        public int GetSurfaceY(int worldX, int worldZ)
        {
            float nx = (worldX + seed) * heightFrequency;
            float nz = (worldZ + seed) * heightFrequency;
            float hBase = FBM2D(nx, nz, 1f, heightOctaves);
            int surface = baseHeight + Mathf.RoundToInt(hBase * heightAmplitude);

            float mMask = Mathf.PerlinNoise(
                (worldX + seed * 11) * mountainFrequency,
                (worldZ + seed * 11) * mountainFrequency
            );

            if (mMask > mountainThreshold)
            {
                float ridge = Ridged2D(worldX + seed * 5, worldZ + seed * 5, mountainFrequency, heightOctaves);
                float t = Mathf.InverseLerp(mountainThreshold, 1f, mMask);
                int add = Mathf.RoundToInt(ridge * mountainAmplitude * t);
                surface += add;
            }

            return Mathf.Clamp(surface, 2, chunkHeight - 2);
        }

        private bool IsCave(int worldX, int y, int worldZ, int surfaceY)
        {
            if (!enableCaves) return false;
            if (y >= caveMaxY) return false;
            if (y >= surfaceY) return false;
            float v = Noise3D(worldX, y, worldZ, caveFrequency, 777);
            return v > caveThreshold;
        }

        private bool IsIronOre(int worldX, int y, int worldZ, int surfaceY)
        {
            if (y < ironMinY || y > ironMaxY) return false;
            if (y >= surfaceY - dirtDepth) return false;

            float v = Noise3D(worldX, y, worldZ, oreFrequency, 222);
            if (v <= oreThreshold) return false;

            float r = Hash01(worldX, y, worldZ, 999);
            return r < ironChance;
        }

        private bool TryGetCellLakeAt(
            int worldX, int worldZ,
            int cellSize,
            float chance,
            int minRadius, int maxRadius,
            int minDepth, int maxDepth,
            int saltBase,
            out int centerX, out int centerZ,
            out int radius, out int depth
        )
        {
            centerX = centerZ = 0;
            radius = 0;
            depth = 0;

            int cs = Mathf.Max(4, cellSize);
            int cellX = Mathf.FloorToInt((float)worldX / cs);
            int cellZ = Mathf.FloorToInt((float)worldZ / cs);

            for (int cx = cellX - 1; cx <= cellX + 1; cx++)
            {
                for (int cz = cellZ - 1; cz <= cellZ + 1; cz++)
                {
                    float spawnRoll = Hash01(cx, 0, cz, saltBase + 0);
                    if (spawnRoll >= chance) continue;

                    float ox = Hash01(cx, 0, cz, saltBase + 1);
                    float oz = Hash01(cx, 0, cz, saltBase + 2);

                    int cWorldX = cx * cs + Mathf.FloorToInt(ox * cs);
                    int cWorldZ = cz * cs + Mathf.FloorToInt(oz * cs);

                    float rr = Hash01(cx, 0, cz, saltBase + 3);
                    int r = minRadius + Mathf.FloorToInt(rr * (maxRadius - minRadius + 1));
                    r = Mathf.Clamp(r, minRadius, maxRadius);

                    float dd = Hash01(cx, 0, cz, saltBase + 4);
                    int d = minDepth + Mathf.FloorToInt(dd * (maxDepth - minDepth + 1));
                    d = Mathf.Clamp(d, minDepth, maxDepth);

                    int dx = worldX - cWorldX;
                    int dz = worldZ - cWorldZ;

                    if (dx * dx + dz * dz <= r * r)
                    {
                        centerX = cWorldX;
                        centerZ = cWorldZ;
                        radius = r;
                        depth = d;
                        return true;
                    }
                }
            }

            return false;
        }

        public void GenerateChunk(Transform parent, int chunkSize)
        {
            chunkBlocks.Clear();

            int parentX0 = Mathf.RoundToInt(parent.position.x);
            int parentZ0 = Mathf.RoundToInt(parent.position.z);

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    int worldX = parentX0 + x;
                    int worldZ = parentZ0 + z;

                    int surfaceY = GetSurfaceY(worldX, worldZ);
                    BiomeType biome = GetBiome(worldX, worldZ);

                    GameObject topPrefab =
                        biome == BiomeType.Desert ? sandPrefab :
                        biome == BiomeType.Snow ? snowPrefab :
                        (grassPrefab != null ? grassPrefab : earthPrefab);

                    bool inLavaLake = false;
                    int lavaCenterX = 0, lavaCenterZ = 0, lavaRadius = 0, lavaDepth = 0;

                    if (biome == BiomeType.Desert && lavaPrefab != null)
                    {
                        inLavaLake = TryGetCellLakeAt(
                            worldX, worldZ,
                            lavaLakeCellSize,
                            lavaLakeChance,
                            lavaLakeMinRadius, lavaLakeMaxRadius,
                            lavaLakeMinDepth, lavaLakeMaxDepth,
                            500,
                            out lavaCenterX, out lavaCenterZ, out lavaRadius, out lavaDepth
                        );
                    }

                    bool inIceLake = false;
                    int iceCenterX = 0, iceCenterZ = 0, iceRadius = 0, iceDepth = 0;

                    if (biome == BiomeType.Snow && icePrefab != null)
                    {
                        inIceLake = TryGetCellLakeAt(
                            worldX, worldZ,
                            iceLakeCellSize,
                            iceLakeChance,
                            iceLakeMinRadius, iceLakeMaxRadius,
                            iceLakeMinDepth, iceLakeMaxDepth,
                            1500,
                            out iceCenterX, out iceCenterZ, out iceRadius, out iceDepth
                        );
                    }

                    int minY = Mathf.Max(0, surfaceY - generateDepthBelowSurface);

                    for (int y = minY; y <= surfaceY; y++)
                    {
                        Vector3 worldPos = parent.position + new Vector3(x, y, z);

                        if (SaveManager.Instance != null && SaveManager.Instance.IsBlockDestroyed(worldPos))
                            continue;

                        if (SaveManager.Instance != null &&
                            SaveManager.Instance.loadedBlocks.TryGetValue(worldPos, out SavedBlock saved))
                        {
                            GameObject savedPrefab = BlockDatabase.Instance.GetPrefab(saved.blockID);
                            if (savedPrefab != null)
                            {
                                var loaded = Instantiate(savedPrefab, worldPos, Quaternion.identity, parent);
                                if (useFaceCulling) chunkBlocks[worldPos] = loaded;
                            }
                            continue;
                        }

                        if (inLavaLake)
                        {
                            int lakeSurfaceY = Mathf.Max(1, surfaceY - 1);
                            int lakeBottomY = Mathf.Max(1, lakeSurfaceY - lavaDepth);

                            if (y > lakeSurfaceY) continue;

                            if (y >= lakeBottomY && y <= lakeSurfaceY)
                            {
                                var lava = Instantiate(lavaPrefab, worldPos, Quaternion.identity, parent);
                                if (useFaceCulling) chunkBlocks[worldPos] = lava;
                                continue;
                            }
                        }

                        if (inIceLake)
                        {
                            int lakeSurfaceY = Mathf.Max(1, surfaceY);
                            int lakeBottomY = Mathf.Max(1, lakeSurfaceY - iceDepth);

                            if (y > lakeSurfaceY) continue;

                            if (y == lakeSurfaceY)
                            {
                                var ice = Instantiate(icePrefab, worldPos, Quaternion.identity, parent);
                                if (useFaceCulling) chunkBlocks[worldPos] = ice;
                                continue;
                            }

                            if (y >= lakeBottomY && y < lakeSurfaceY)
                                continue;
                        }

                        if (y > 0 && y < surfaceY && IsCave(worldX, y, worldZ, surfaceY))
                            continue;

                        GameObject prefabToUse;

                        if (y == 0) prefabToUse = bedRockPrefab;
                        else if (y == surfaceY) prefabToUse = topPrefab;
                        else if (y >= surfaceY - dirtDepth) prefabToUse = earthPrefab;
                        else
                        {
                            prefabToUse = stonePrefab;
                            if (ironPrefab != null && IsIronOre(worldX, y, worldZ, surfaceY))
                                prefabToUse = ironPrefab;
                        }

                        GameObject block = Instantiate(prefabToUse, worldPos, Quaternion.identity, parent);
                        if (useFaceCulling) chunkBlocks[worldPos] = block;
                    }

                    Vector3 surfacePos = parent.position + new Vector3(x, surfaceY, z);

                    if (biome == BiomeType.Forest && woodPrefab != null && bushPrefab != null)
                    {
                        if (Hash01(worldX, surfaceY, worldZ, 100) < treeChanceForest)
                        {
                            Vector3 trunkBase = surfacePos + Vector3.up;

                            for (int i = 0; i < 3; i++)
                            {
                                Vector3 trunkPos = trunkBase + Vector3.up * i;
                                if (SaveManager.Instance != null && SaveManager.Instance.loadedBlocks.ContainsKey(trunkPos))
                                    continue;

                                var trunk = Instantiate(woodPrefab, trunkPos, Quaternion.identity, parent);
                                if (useFaceCulling) chunkBlocks[trunkPos] = trunk;
                            }

                            Vector3 leavesBase = trunkBase + Vector3.up * 3;
                            for (int h = 0; h < 2; h++)
                            {
                                for (int dx = -1; dx <= 1; dx++)
                                {
                                    for (int dz = -1; dz <= 1; dz++)
                                    {
                                        Vector3 leafPos = leavesBase + new Vector3(dx, h, dz);
                                        if (SaveManager.Instance != null && SaveManager.Instance.loadedBlocks.ContainsKey(leafPos))
                                            continue;

                                        var leaf = Instantiate(bushPrefab, leafPos, Quaternion.identity, parent);
                                        if (useFaceCulling) chunkBlocks[leafPos] = leaf;
                                    }
                                }
                            }
                        }

                        if (bushPrefab != null && Hash01(worldX, surfaceY, worldZ, 101) < bushChanceForest)
                        {
                            Vector3 bushPos = surfacePos + Vector3.up;
                            if (SaveManager.Instance == null || !SaveManager.Instance.loadedBlocks.ContainsKey(bushPos))
                            {
                                var bush = Instantiate(bushPrefab, bushPos, Quaternion.identity, parent);
                                if (useFaceCulling) chunkBlocks[bushPos] = bush;
                            }
                        }
                    }
                    else if (biome == BiomeType.Snow)
                    {
                        GameObject snowBushToUse = snowyBushPrefab != null ? snowyBushPrefab : bushPrefab;

                        if (snowBushToUse != null && Hash01(worldX, surfaceY, worldZ, 102) < bushChanceSnow)
                        {
                            Vector3 bushPos = surfacePos + Vector3.up;
                            if (SaveManager.Instance == null || !SaveManager.Instance.loadedBlocks.ContainsKey(bushPos))
                            {
                                var bush = Instantiate(snowBushToUse, bushPos, Quaternion.identity, parent);
                                if (useFaceCulling) chunkBlocks[bushPos] = bush;
                            }
                        }
                    }
                    else if (biome == BiomeType.Desert)
                    {
                        if (cactusPrefab != null && Hash01(worldX, surfaceY, worldZ, 103) < cactusChanceDesert)
                        {
                            int cactusH = cactusMinHeight +
                                          Mathf.FloorToInt(Hash01(worldX, surfaceY, worldZ, 104) *
                                                           (cactusMaxHeight - cactusMinHeight + 1));
                            cactusH = Mathf.Clamp(cactusH, cactusMinHeight, cactusMaxHeight);

                            for (int i = 1; i <= cactusH; i++)
                            {
                                Vector3 cPos = surfacePos + Vector3.up * i;
                                if (SaveManager.Instance != null && SaveManager.Instance.loadedBlocks.ContainsKey(cPos))
                                    continue;

                                var cactus = Instantiate(cactusPrefab, cPos, Quaternion.identity, parent);
                                if (useFaceCulling) chunkBlocks[cPos] = cactus;
                            }
                        }
                    }
                }
            }

            if (useFaceCulling) ApplyFaceCulling();
        }

        private void ApplyFaceCulling()
        {
            foreach (var kvp in chunkBlocks)
            {
                Vector3 pos = kvp.Key;
                GameObject block = kvp.Value;
                if (block == null) continue;

                MeshRenderer[] renderers = block.GetComponentsInChildren<MeshRenderer>(true);
                if (renderers == null || renderers.Length == 0) continue;

                bool hasTop = chunkBlocks.ContainsKey(pos + Vector3.up);
                bool hasBottom = chunkBlocks.ContainsKey(pos + Vector3.down);
                bool hasRight = chunkBlocks.ContainsKey(pos + Vector3.right);
                bool hasLeft = chunkBlocks.ContainsKey(pos + Vector3.left);
                bool hasForward = chunkBlocks.ContainsKey(pos + Vector3.forward);
                bool hasBack = chunkBlocks.ContainsKey(pos + Vector3.back);

                if (hasTop && hasBottom && hasRight && hasLeft && hasForward && hasBack)
                {
                    foreach (var r in renderers) r.enabled = false;
                }
            }
        }
    }
}
