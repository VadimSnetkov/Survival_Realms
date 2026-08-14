using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BlockBuildingCraftingSystem
{
    public class HeroPlayerScript : MonoBehaviour
    {
        public static HeroPlayerScript Instance;

        public FirstPersonController firstPersonController;
        public CharacterController characterController;
        public float Health = 100;
        [HideInInspector]
        public float TotalHealth = 100;
        public float Stamina = 1000;
        [HideInInspector]
        public float TotalStamina = 1000;
        [HideInInspector]
        public GameObject FPSHands;

        public GameObject MainCamera;
        public CapsuleCollider CapsuleCollider;
        public AudioSource AudioSource;

        void Start()
        {
            TotalHealth = Health;
            TotalStamina = Stamina;

            Time.timeScale = 1;
            GameCanvas.Instance.UpdateHealth();
            GameCanvas.Instance.UpdateStamina();
        }

        public void GetDamage(int Damage)
        {
            Health = Health - Damage;
            if (Health < 0) Health = 0;
            GameCanvas.Instance.UpdateHealth();
            if (Health <= 0)
            {
                firstPersonController.enabled = false;
                characterController.enabled = false;
                AudioManager.Instance.Play_Pain();
                GameCanvas.Instance.Show_GameOverPanel();
                HeroPlayerScript.Instance.FPSHands.SetActive(false);
            }
            else
            {
                CameraScript.Instance.fCamShakeImpulse = 0.2f;
            }
        }

        private void Awake()
        {
            Instance = this;
        }


        public void Heal()
        {
            Health = Health + 50;
            if (Health > TotalHealth) Health = 100;
            GameCanvas.Instance.UpdateHealth();
        }

        public void Rest()
        {
            Stamina = Stamina + 50;
            if (Stamina > TotalStamina) Stamina = 100;
            GameCanvas.Instance.UpdateStamina();
        }

        public void ActivatePlayer()
        {
            transform.eulerAngles = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            firstPersonController.enabled = true;
            characterController.enabled = true;
            transform.eulerAngles = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }

        public void DeactivatePlayer()
        {
            firstPersonController.enabled = false;
            characterController.enabled = false;
        }

        public void ChangeTag(bool hide)
        {
            if (hide)
            {
                gameObject.tag = "Untagged";
            }
            else
            {
                gameObject.tag = "Player";
            }
        }

        public void ActivatePlayerInputs()
        {
            firstPersonController.enabled = true;
            characterController.enabled = true;
        }
    }
}