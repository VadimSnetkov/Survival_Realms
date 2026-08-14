using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        public AudioClip Audio_Jump;
        public AudioClip[] Audio_Pain;
        public AudioSource audioSource;
        public AudioClip Audio_Click;

        public AudioSource AudioSource_Ambience;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Play_Pain()
        {
            audioSource.pitch = 1;
            audioSource.PlayOneShot(Audio_Pain[Random.Range(0, Audio_Pain.Length)]);
        }

        public void Play_Jump()
        {
            audioSource.pitch = 1;
            audioSource.PlayOneShot(Audio_Jump);
        }

        public void Play_Button_Click()
        {
            audioSource.PlayOneShot(Audio_Click);
        }
    }
}