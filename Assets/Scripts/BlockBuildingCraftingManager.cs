using System.Collections;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class BlockBuildingCraftingManager : MonoBehaviour
    {
        public GameObject CameraMain;
        public static BlockBuildingCraftingManager Instance;
        public PlayerValues playerValues;
        public ControllerType controllerType;

        public void UpdatePlayerValues()
        {
            playerValues.Health = HeroPlayerScript.Instance.Health;
        }


        private void Awake()
        {
            Instance = this;
            playerValues = new PlayerValues();
        }


        void Start()
        {
            GameCanvas.Instance.Crosshair.SetActive(true);
            if(controllerType == ControllerType.Mobile)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            Application.targetFrameRate = 60;
            UpdatePlayerValues();
            StartCoroutine(DailyStart());
        }


        public IEnumerator DailyStart()
        {
            CameraMain.SetActive(true);
            GameCanvas.Instance.image_Blinking.gameObject.SetActive(true);
            GameCanvas.Instance.image_Blinking.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(1);
            FirstPersonController.Instance.enabled = true;
            CameraScript.Instance.enabled = true;
        }
    }

    public class PlayerValues
    {
        public float Health;
    }

    public enum ControllerType
    {
        PC,
        Mobile
    }
}