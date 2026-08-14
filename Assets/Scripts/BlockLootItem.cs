using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class BlockLootItem : MonoBehaviour
    {
        public string itemID = "dirt";
        public int quantity = 1;

        public float pickupDistance = 2f;
        public float rotateSpeed = 50f;
        private bool canBeCollected = false;
        [HideInInspector]
        public Rigidbody rigid;
        [HideInInspector]
        public Collider cold;
        [Header("Pickup FX")]
        public AudioClip pickupSound;

        void Start()
        {
            rigid = GetComponent<Rigidbody>();
            cold = GetComponent<Collider>();
            Invoke(nameof(EnableCollection), 0.5f);
        }

        void EnableCollection()
        {
            canBeCollected = true;
            if (rigid != null) Destroy(rigid);
            if (cold != null) Destroy(cold);
        }

        void Update()
        {
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

            if (canBeCollected)
            {
                float distance = Vector3.Distance(transform.position, HeroPlayerScript.Instance.transform.position);
                if (distance < pickupDistance)
                {
                    transform.position = Vector3.MoveTowards(transform.position, HeroPlayerScript.Instance.transform.position, 5 * Time.deltaTime);
                    if (distance < 0.5f)
                    {
                        InventoryManager.Instance.AddItem(itemID, quantity);

                        if (pickupSound != null)
                        {
                            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                        }

                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}