using UnityEngine;
using System.Threading.Tasks;

namespace TK.Blast
{
    public class ParticleManager : SingletonBehaviour<ParticleManager>
    {
        [SerializeField] private ParticleSystem celebrationParticles;

        public static async Task PlayCelebrationAsync()
        {
            if (!Instance?.celebrationParticles)
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
    }
}