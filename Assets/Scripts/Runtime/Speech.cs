using UnityEngine;

namespace WordGarden
{
    // Android native offline-capable TextToSpeech. Android voice packs may need installing.
    // In the Editor the spoken word is shown as a readable fallback.
    public sealed class Speech
    {
        private AndroidJavaObject engine;
        private bool attempted;
        public bool Speak(string words)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
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
