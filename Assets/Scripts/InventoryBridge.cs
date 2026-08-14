using UnityEngine;
using UnityEngine.InputSystem;

namespace BlockBuildingCraftingSystem
{
    public class InventoryBridge : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryRoot;

        private InputAction invAction;

        void Awake()
        {
            invAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");
            invAction.Enable();
        }

        void Update()
        {
            if (invAction.WasPressedThisFrame())
                ToggleInventory();
        }

        public void ToggleInventory()
        {
            bool open = !inventoryRoot.activeSelf;
            inventoryRoot.SetActive(open);

            if (open)
            {
                HeroPlayerScript.Instance.DeactivatePlayer();
                if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.PC)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
                if (CameraScript.Instance != null) CameraScript.Instance.enabled = false;
            }
            else
            {
                HeroPlayerScript.Instance.ActivatePlayer();
                if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.PC)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                if (CameraScript.Instance != null) CameraScript.Instance.enabled = true;
            }
        }
    }
}
