using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BlockBuildingCraftingSystem
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject Panel_Settings;
        public Button Button_Continue;
        public string SceneName = "Mixed_GamePlay";

        private void Start()
        {
            string savePath = Application.persistentDataPath + "/world.json";
            string inventoryPath = Application.persistentDataPath + "/inventory_save.json";

            if (File.Exists(savePath) || File.Exists(inventoryPath))
            {
                Button_Continue.interactable = true;
            }
            else
            {
                Button_Continue.interactable = false;
            }
        }

        public void Click_Quit()
        {
            Application.Quit();
        }

        public void Click_Continue()
        {
            SceneManager.LoadScene(SceneName);
        }

        public void Click_NewGame()
        {
            string worldPath = Application.persistentDataPath + "/world.json";
            string inventoryPath = Application.persistentDataPath + "/inventory_save.json";

            if (File.Exists(worldPath))
            {
                File.Delete(worldPath);
                Debug.Log("World save deleted.");
            }

            if (File.Exists(inventoryPath))
            {
                File.Delete(inventoryPath);
                Debug.Log("Inventory save deleted.");
            }
            SceneManager.LoadScene(SceneName);
        }

        public void Click_Settings()
        {
            Panel_Settings.SetActive(true);
        }

        public void Click_Settings_Close()
        {
            Panel_Settings.SetActive(false);
        }
    }
}
