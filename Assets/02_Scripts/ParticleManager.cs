using UnityEngine;
using System.Threading.Tasks;

namespace TK.Blast
{
    public class ParticleManager : MonoBehaviour
    {
        [SerializeField] private ParticleSystem celebrationParticles;
        private static ParticleManager Instance { get; set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static async Task PlayCelebrationAsync()
        {
            if (Instance?.celebrationParticles == null)
            {
                Debug.LogWarning("Celebration particles not assigned!");
                return;
            }

            Instance.celebrationParticles.gameObject.SetActive(true);
            Instance.celebrationParticles.Play();

            // Wait for particles to finish
            await Task.Delay((int)(Instance.celebrationParticles.main.duration * 1000));
            Instance.celebrationParticles.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}