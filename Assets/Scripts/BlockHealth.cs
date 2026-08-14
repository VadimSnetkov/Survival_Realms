using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class BlockHealth : MonoBehaviour
    {
        [Header("Block Info")]
        public string blockID = "";
        public int maxHealth = 5;

        private int currentHealth;
        private Renderer rend;

        private MaterialPropertyBlock mpb;

        [Header("Break FX")]
        public AudioClip breakSound;
        public GameObject breakParticlesPrefab;
        public GameObject lootItemPrefab;
        public Color particleHitColor = Color.gray;

        void Awake()
        {
            currentHealth = maxHealth;
            rend = GetComponentInChildren<Renderer>();

            mpb = new MaterialPropertyBlock();
        }

        void Start()
        {
            UpdateCrackVisual();
        }

        public void TakeHit(int damage)
        {
            currentHealth = currentHealth - damage;

            if (currentHealth <= 0)
            {
                PlayEffects();

                if (SaveManager.Instance.HasBlock(transform.position))
                    SaveManager.Instance.UnregisterBlock(transform.position);

                SaveManager.Instance.RegisterDestroyedBlock(transform.position);

                UpdateNeighborBlocksVisibility(transform.position);

                BlockInteraction.Instance.breakParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Destroy(gameObject);
            }

            else
            {
                UpdateCrackVisual();
            }
        }

        public void ResetHealth()
        {
            if (currentHealth != maxHealth)
            {
                currentHealth = maxHealth;
                UpdateCrackVisual();
            }
        }

        void UpdateCrackVisual()
        {
            float ratio = 1f - ((float)currentHealth / maxHealth);

            rend.GetPropertyBlock(mpb);

            mpb.SetFloat("_CrackAmount", ratio);

            rend.SetPropertyBlock(mpb);
        }

        void PlayEffects()
        {
            if (breakSound != null)
                AudioSource.PlayClipAtPoint(breakSound, transform.position);

            if (breakParticlesPrefab != null)
            {
                GameObject fx = Instantiate(breakParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 3f);
            }

            if (lootItemPrefab != null)
                Instantiate(lootItemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        void UpdateNeighborBlocksVisibility(Vector3 position)
        {
            Vector3[] neighbors = new Vector3[]
            {
                position + Vector3.up,
                position + Vector3.down,
                position + Vector3.right,
                position + Vector3.left,
                position + Vector3.forward,
                position + Vector3.back
            };

            LayerMask blockLayer = ~0;
            
            foreach (Vector3 neighborPos in neighbors)
            {
                Collider[] hits = Physics.OverlapSphere(neighborPos, 0.3f, blockLayer);
                
                foreach (Collider hit in hits)
                {
                    if (hit != null && hit.gameObject != null && hit.gameObject != this.gameObject)
                    {
                        float distance = Vector3.Distance(hit.transform.position, neighborPos);
                        if (distance < 0.5f)
                        {
                            MeshRenderer[] renderers = hit.GetComponentsInChildren<MeshRenderer>(true);
                            
                            foreach (MeshRenderer renderer in renderers)
                            {
                                renderer.enabled = true;
                            }
                            
                            break;
                        }
                    }
                }
            }
        }

        bool ShouldBlockBeVisible(Vector3 position)
        {
            return true;
        }
    }
}