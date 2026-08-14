using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace BlockBuildingCraftingSystem
{
    public class GameCanvas : MonoBehaviour
    {
        public static GameCanvas Instance;
        public Image image_Blinking;
        public LayerMask layerMaskForInteract;
        public GameObject Panel_GameUI;
        public GameObject Panel_Pause;
        public GameObject Panel_Settings;
        public Image Image_Health;
        public Image Image_Stamina;
        [HideInInspector]
        public GameObject LastClickedArea;
        [HideInInspector]
        public bool isPaused = false;
        public GameObject Crosshair;
        public Text Text_Map;
        public GameObject Joystick_Mobile;
        public GameObject Touchpad_Mobile;
        public GameObject Button_Pause;
        public GameObject Button_Crafting;
        public GameObject Button_Jump;

        private InputAction escapeAction;
        private InputAction mapAction;

        private void Awake()
        {
            Instance = this;
            escapeAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
            escapeAction.AddBinding("<Gamepad>/start");
            escapeAction.Enable();

            mapAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/m");
            mapAction.AddBinding("<Gamepad>/buttonWest");
            mapAction.Enable();
        }

        public void Click_BacktoMenu()
        {
            Application.Quit();
        }

        public void UpdateStatus()
        {
            BlockBuildingCraftingManager.Instance.UpdatePlayerValues();
        }


        public void Blink()
        {
            image_Blinking.GetComponent<Animation>().Play();
        }

        public void Click_ButtonPause()
        {
            if (isPaused)
            {
                Click_Continue();
            }
            else
            {
                Click_Pause();
            }
        }

        public void Click_Continue()
        {
            if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.PC)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            HeroPlayerScript.Instance.ActivatePlayerInputs();
            HeroPlayerScript.Instance.ActivatePlayer();
            CameraScript.Instance.enabled = true;
            Time.timeScale = 1;
            isPaused = false;
            Panel_Pause.SetActive(false);
            Panel_Settings.SetActive(false);
            Panel_GameUI.SetActive(true);
        }

        public void Click_Pause()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            HeroPlayerScript.Instance.DeactivatePlayer();
            CameraScript.Instance.enabled = false;
            Time.timeScale = 0;
            isPaused = true;
            Panel_Pause.SetActive(true);
            Panel_GameUI.SetActive(false);
        }

        public void UpdateHealth()
        {
            Image_Health.fillAmount = (HeroPlayerScript.Instance.Health / HeroPlayerScript.Instance.TotalHealth);
        }

        public void UpdateStamina()
        {
            Image_Stamina.fillAmount = (HeroPlayerScript.Instance.Stamina / HeroPlayerScript.Instance.TotalStamina);
        }

        void Update()
        {
            if (escapeAction.WasPressedThisFrame())
            {
                if (CraftingManager.Instance.panelCrafting.activeSelf)
                {

                    CraftingManager.Instance.HideCraftingPanel();
                    return;
                }

                if (MapPanel.activeSelf)
                {
                    MapPanel.SetActive(false);
                    MapPanelCamera.SetActive(false);
                    HeroPlayerScript.Instance.ActivatePlayer();
                    CameraScript.Instance.enabled = true;
                    return;
                }


                if (isPaused)
                {
                    Click_Continue();
                }
                else
                {
                    Click_Pause();
                }
            }
            else if (mapAction.WasPressedThisFrame())
            {
                ToogleMap();
            }
        }

        public void ToogleMap()
        {
            if (MapPanel.activeSelf)
            {
                MapPanel.SetActive(false);
                MapPanelCamera.SetActive(false);
                if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.PC)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                HeroPlayerScript.Instance.ActivatePlayer();
                CameraScript.Instance.enabled = true;
            }
            else
            {
                MapPanel.SetActive(true);
                if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.PC)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = true;
                }
                MapPanelCamera.SetActive(true);
                HeroPlayerScript.Instance.DeactivatePlayer();
                CameraScript.Instance.enabled = false;
            }
        }

        public GameObject MapPanel;
        public GameObject MapPanelCamera;

        public void Show_GameOverPanel()
        {
            Debug.Log("Game Over...");
        }



        public void Click_Settings()
        {
            Panel_Settings.SetActive(true);
            Panel_Pause.SetActive(false);
        }

        public void Click_Close_Settings()
        {
            Panel_Settings.SetActive(false);
            Panel_Pause.SetActive(true);
        }

        public void Click_ShowNote()
        {
            Panel_GameUI.SetActive(false);
            HeroPlayerScript.Instance.DeactivatePlayer();
        }

        private void Start()
        {
            UpdateStatus();
            if (BlockBuildingCraftingManager.Instance.controllerType == ControllerType.Mobile)
            {
                Text_Map.text = "Map";
                Joystick_Mobile.SetActive(true);
                Touchpad_Mobile.SetActive(true);
                Button_Pause.SetActive(true);
                Button_Crafting.SetActive(true);
                Button_Jump.SetActive(true);
            }
            else
            {
                Text_Map.text = "Map (M)";
                Joystick_Mobile.SetActive(false);
                Touchpad_Mobile.SetActive(false);
                Button_Pause.SetActive(false);
                Button_Crafting.SetActive(false);
                Button_Jump.SetActive(false);
            }
        }

        public void Click_Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Show_GameUI()
        {
            Panel_GameUI.SetActive(true);
        }

        public void Hide_GameUI()
        {
            Panel_GameUI.SetActive(false);
        }
    }
}