using BlockBuildingCraftingSystem;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class DamagerBlock : MonoBehaviour
    {
        public int Damage = 2;
        public float DamageInterval = 1f;
        private float lastDamageTime = 0f;

        void Start()
        {
            
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (Time.time - lastDamageTime >= DamageInterval)
                {
                    HeroPlayerScript.Instance.GetDamage(Damage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}
