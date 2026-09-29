using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace WordGarden
{
    // Android native offline-capable TextToSpeech. Android voice packs may need installing.
    // In the Editor the spoken word is shown as a readable fallback.
    public sealed class Speech
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern int WG_Speak(string words);
#endif
        private AndroidJavaObject engine;
        private bool attempted;
        public bool Speak(string words)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { return WG_Speak(words) != 0; }
            catch (System.Exception ex) { Debug.LogWarning("Browser speech unavailable: " + ex.Message); return false; }
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (!attempted)
                {
                    attempted = true;
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        engine = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new InitListener());
                        using (var locale = new AndroidJavaClass("java.util.Locale"))
                        {
                            engine.Call<int>("setLanguage", locale.GetStatic<AndroidJavaObject>("US"));
                        }
                    }
                }
                engine.Call<int>("speak", words, 0, null, "wg-voice");
                return true;
            }
            catch (System.Exception ex) { Debug.LogWarning("Speech unavailable: " + ex.Message); return false; }
#else
            return false;
#endif
        }
        public void Dispose() { if (engine != null) { engine.Call("shutdown"); engine.Dispose(); engine = null; } }
#if UNITY_ANDROID && !UNITY_EDITOR
        class InitListener : AndroidJavaProxy
        {
            public InitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") {}
            public void onInit(int status) { Debug.Log("TTS status " + status); }
        }
#endif
    }
}
