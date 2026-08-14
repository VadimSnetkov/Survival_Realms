using InventoryFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace BlockBuildingCraftingSystem
{
    public class BlockInteraction : MonoBehaviour
    {
        public Camera playerCamera;
        public GameObject blockPrefab;
        public float maxDistance = 5f;
        public float hitDelay = 0.25f;

        private BlockHealth currentTarget;
        private MobAI currentMobTarget;
        private float lastHitTime;
        public AudioClip Audio_Building;
        public AudioSource AudioSource_Digging;
        public ParticleSystem breakParticle;

        public static BlockInteraction Instance;
        private InputAction mouseLeftAction;
        private InputAction mouseRightAction;

        private void Awake()
        {
            Instance = this;
            mouseLeftAction = new InputAction(type: InputActionType.Button);
            mouseLeftAction.AddBinding("<Mouse>/leftButton");
            mouseLeftAction.AddBinding("<Touchscreen>/press");
            mouseLeftAction.AddBinding("<Gamepad>/rightTrigger");
            mouseLeftAction.Enable();

            mouseRightAction = new InputAction(type: InputActionType.Button);
            mouseRightAction.AddBinding("<Mouse>/rightButton");
            mouseRightAction.AddBinding("<Gamepad>/leftTrigger");
            mouseRightAction.Enable();
        }
        bool canContinue = false;
        float timeStartClicking = 0;
        bool breakinganim = false;

        void Update()
        {
            if (GameCanvas.Instance.isPaused) return;
            if (GameCanvas.Instance.MapPanel.activeSelf) return;
            if (CraftingManager.Instance.panelCrafting.activeSelf) return;
            breakinganim = false;
            
            CheckTargetBlock();
            
            if (mouseLeftAction.WasPressedThisFrame() || mouseRightAction.WasPressedThisFrame())
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    canContinue = false;
                }
                else
                {
                    canContinue = true;
                    timeStartClicking = Time.time;
                }
            }

            if (!canContinue) return;

            if(mouseLeftAction.WasReleasedThisFrame() && Time.time < timeStartClicking + 0.1f)
            {
                if(BlockBuildingCraftingManager.Instance.controllerType == ControllerType.Mobile)
                {
                    TryPlaceBlock();
                }
            }
            else if (mouseLeftAction.IsPressed())
            {
                if (Time.time - lastHitTime >= hitDelay && Time.time > timeStartClicking + 0.15f)
                {
                    if (currentMobTarget != null)
                    {
                        Debug.Log("AttackMob çaðrýlýyor!");
                        AttackMob();
                    }
                    else if (currentTarget != null)
                    {
                        BreakBlock();
                    }
                    
                    lastHitTime = Time.time;
                }
            }
            FPSHandRotator.Instance.animationHand.SetBool("Breaking", (breakinganim && (currentTarget != null || currentMobTarget != null)));

            if (mouseRightAction.WasPressedThisFrame())
            {
                TryPlaceBlock();
            }
        }

        void BreakBlock()
        {
            breakParticle.transform.position = currentTarget.transform.position;
            if(FPSHandRotator.Instance.currentItem != null)
            {
                currentTarget.TakeHit(FPSHandRotator.Instance.currentItem.Damage);
                FPSHandRotator.Instance.currentItem.TakeDamage();
            }
            else
            {
                currentTarget.TakeHit(1);
            }
            var main = breakParticle.main;
            main.startColor = currentTarget.particleHitColor;
            breakParticle.Play();
            breakinganim = true;
            if (!AudioSource_Digging.isPlaying)
            {
                AudioSource_Digging.pitch = Random.Range(0.5f, 1.5f);
                AudioSource_Digging.Play();
            }
        }

        void AttackMob()
        {
            float damage = 10f;
            if(FPSHandRotator.Instance.currentItem != null)
            {
                damage = FPSHandRotator.Instance.currentItem.Damage;
                FPSHandRotator.Instance.currentItem.TakeDamage();
            }

            currentMobTarget.TakeDamage(damage);
            
            breakinganim = true;
            
            if (!AudioSource_Digging.isPlaying)
            {
                AudioSource_Digging.pitch = Random.Range(0.8f, 1.2f);
                AudioSource_Digging.Play();
            }
        }

        void CheckTargetBlock()
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance))
            {
                Debug.Log("Hit: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.tag);
                
                if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Character"))
                {
                    var newMobTarget = hit.collider.GetComponent<MobAI>();
                    
                    if (newMobTarget != null)
                    {
                        Debug.Log("Mob hedef bulundu: " + newMobTarget.gameObject.name);
                    }
                    else
                    {
                        Debug.LogWarning("MobAI component bulunamadý!");
                    }
                    
                    if (currentTarget != null)
                    {
                        currentTarget.ResetHealth();
                        currentTarget = null;
                    }
                    
                    currentMobTarget = newMobTarget;
                }
                else
                {
                    var newTarget = hit.collider.GetComponent<BlockHealth>();

                    if (currentTarget != null && newTarget != currentTarget)
                        currentTarget.ResetHealth();

                    currentTarget = newTarget;
                    
                    currentMobTarget = null;
                }
            }
            else
            {
                if (currentTarget != null)
                {
                    currentTarget.ResetHealth();
                }
                currentTarget = null;
                currentMobTarget = null;
            }
        }

        void TryPlaceBlock()
        {
            if (InventoryManager.Instance.items == null || InventoryManager.Instance.items.Count == 0)
                return;

            GameObject blockPrefab = InventoryManager.Instance.GetSelectedBlockPrefab();
            if (blockPrefab == null) return;

            var selectedItem = InventoryManager.Instance.GetSelectedHotbarItem();
            if (selectedItem != null)
            {
                var blockData = BlockDatabase.Instance.blocks
                    .Find(b => b.id == selectedItem.id);

                if (blockData == null || blockData.type != BlockType.Placeable)
                    return;
            }
            else
            {
                return;
            }

            string blockID = InventoryManager.Instance.GetSelectedBlockID();
            if (!InventoryManager.Instance.HasItem(blockID)) return;

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance))
            {
                Vector3 placePosition = hit.collider.transform.position + hit.normal;
                placePosition = new Vector3(
                    Mathf.Round(placePosition.x),
                    Mathf.Round(placePosition.y),
                    Mathf.Round(placePosition.z)
                );

                GameObject newBlock = Instantiate(blockPrefab, placePosition, Quaternion.identity);

                BlockHealth bh = newBlock.GetComponent<BlockHealth>();
                if (bh != null)
                {
                    bh.blockID = blockID;
                    SaveManager.Instance.RegisterBlock(blockID, placePosition);
                }

                InventoryManager.Instance.RemoveItem(blockID, 1);
                AudioSource.PlayClipAtPoint(Audio_Building, transform.position);
            }
        }


    }
}