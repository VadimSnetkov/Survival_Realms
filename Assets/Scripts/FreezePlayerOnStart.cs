using System.Collections;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class FreezePlayerOnStart : MonoBehaviour
    {
        [Header("Freeze Time")]
        public float freezeSeconds = 3f;

        [Header("References (optional)")]
        public FirstPersonController firstPersonController;
        public CameraScript cameraScript;
        public CharacterController characterController;

        private Vector3 lockedPosition;
        private Quaternion lockedRotation;

        private void Awake()
        {
            // Auto-find if not assigned
            if (!firstPersonController) firstPersonController = GetComponent<FirstPersonController>();
            if (!characterController) characterController = GetComponent<CharacterController>();
            if (!cameraScript) cameraScript = FindFirstObjectByType<CameraScript>();
        }

        private void Start()
        {
            StartCoroutine(FreezeRoutine());
        }

        private IEnumerator FreezeRoutine()
        {
            // lock current transform
            lockedPosition = transform.position;
            lockedRotation = transform.rotation;

            // disable controls
            if (firstPersonController) firstPersonController.enabled = false;
            if (cameraScript) cameraScript.enabled = false;

            // disable CharacterController so it doesn't jitter / slide
            if (characterController) characterController.enabled = false;

            float end = Time.time + freezeSeconds;
            while (Time.time < end)
            {
                // keep player perfectly fixed during generation spikes
                transform.position = lockedPosition;
                transform.rotation = lockedRotation;
                yield return null;
            }

            // re-enable
            if (characterController) characterController.enabled = true;
            if (cameraScript) cameraScript.enabled = true;
            if (firstPersonController) firstPersonController.enabled = true;
        }
    }
}
