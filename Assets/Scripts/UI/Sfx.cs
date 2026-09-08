using System.Collections.Generic;
using UnityEngine;

namespace IL6
{
    /// <summary>UI SFX adapter backed only by the reviewed CC0 runtime clips.</summary>
    public static class Sfx
    {
        private static AudioSource source;
        private static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        private static AudioSource Source()
        {
            if (source != null) return source;
            var go = new GameObject("__SfxRunner");
            Object.DontDestroyOnLoad(go);
            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            return source;
        }

        public static float Volume
        {
            get => PlayerPrefs.GetFloat("il6_sfx_vol", 0.6f);
            set { PlayerPrefs.SetFloat("il6_sfx_vol", Mathf.Clamp01(value)); PlayerPrefs.Save(); }
        }

        public static void Hit() => Play("action_primary", 0.70f);
        public static void Pickup() => Play("result_success", 0.50f);
        public static void Death() => Play("result_failure", 0.60f);
        public static void Build() => Play("transition", 0.55f);
        public static void Click() => Play("ui_click", 0.40f);
        public static void NightHowl() => Play("danger_warning", 0.45f);
        public static void Boss() => Play("danger_warning", 0.70f);

        private static void Play(string cue, float gain)
        {
            if (!clips.TryGetValue(cue, out var clip))
            {
                clip = Resources.Load<AudioClip>("Audio/generated/" + cue);
                clips[cue] = clip;
            }
            if (clip != null) Source().PlayOneShot(clip, gain * Volume);
        }
    }
}
