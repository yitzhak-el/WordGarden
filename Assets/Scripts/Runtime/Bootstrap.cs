using UnityEngine;
using UnityEngine.SceneManagement;

namespace WordGarden
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Start()
        {
            if (Object.FindObjectOfType<GameController>() != null) return;
            var host = new GameObject("Word Garden");
            host.AddComponent<GameController>();
        }
    }
}
