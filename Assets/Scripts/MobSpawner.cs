using System.Collections.Generic;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class MobSpawner : MonoBehaviour
    {
        [Header("Mob Settings")]
        public List<GameObject> mobPrefabs = new List<GameObject>();
        public List<GameObject> spawnedMobs = new List<GameObject>();
        
        [Header("Spawn Settings")]
        public Transform playerTransform;
        public float spawnPeriod = 10f;
        public int maxSpawnedMobs = 20;
        
        [Header("Spawn Range")]
        public float minSpawnDistance = 15f;
        public float maxSpawnDistance = 30f;
        public float spawnHeight = 50f;

        private float spawnTimer = 0f;
        private float cleanupTimer = 0f;
        private float cleanupInterval = 5f;

        void Start()
        {
            if (playerTransform == null && HeroPlayerScript.Instance != null)
            {
                playerTransform = HeroPlayerScript.Instance.transform;
            }
        }

        void Update()
        {
            if (playerTransform == null) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnPeriod)
            {
                spawnTimer = 0f;
                TrySpawnMob();
            }

            cleanupTimer += Time.deltaTime;
            if (cleanupTimer >= cleanupInterval)
            {
                cleanupTimer = 0f;
                CleanupNullMobs();
            }
        }

        void TrySpawnMob()
        {
            if (spawnedMobs.Count >= maxSpawnedMobs)
            {
                return;
            }

            if (mobPrefabs.Count == 0)
            {
                return;
            }

            GameObject randomMobPrefab = mobPrefabs[Random.Range(0, mobPrefabs.Count)];
            
            MobAI mobAI = randomMobPrefab.GetComponent<MobAI>();
            if (mobAI != null)
            {
                if (mobAI.spawnsAtOnlyNight)
                {
                    if (DayNightManager.Instance == null || !DayNightManager.Instance.isDark)
                    {
                        return;
                    }
                }
            }

            Vector3 spawnPos = GetRandomSpawnPosition();
            
            if (spawnPos != Vector3.zero)
            {
                GameObject spawnedMob = Instantiate(randomMobPrefab, spawnPos, Quaternion.identity);
                spawnedMobs.Add(spawnedMob);
            }
        }

        Vector3 GetRandomSpawnPosition()
        {
            float randomAngle = Random.Range(0f, 360f);
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 randomDirection = new Vector3(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                0,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );

            Vector3 horizontalPos = playerTransform.position + randomDirection * randomDistance;
            
            Vector3 rayStart = new Vector3(horizontalPos.x, playerTransform.position.y + spawnHeight, horizontalPos.z);
            RaycastHit hit;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, spawnHeight + 100f))
            {
                return hit.point + Vector3.up * 0.5f;
            }

            return Vector3.zero;
        }

        void CleanupNullMobs()
        {
            spawnedMobs.RemoveAll(mob => mob == null);
        }

        void OnDrawGizmosSelected()
        {
            if (playerTransform == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerTransform.position, minSpawnDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, maxSpawnDistance);
        }
    }
}
