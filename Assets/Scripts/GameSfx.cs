using System.Collections.Generic;
using UnityEngine;

// Short sounds use a shared pool so a pickup can finish ringing after it disappears.
// The pool belongs to the level and is unloaded together with it.
public sealed class GameSfx : MonoBehaviour
{
    public enum Cue
    {
        KeyPickup, DoorUnlock, CabbageBite, SnailMove, LadybugMove, LadybugAttack,
        SpiderMove, SpiderAttack, FrogSpit, SpikeRise, FlameBurn
    }

    struct Sound
    {
        public AudioClip clip, alternate;
        public float gain, range;
        public bool useAlternate;
    }

    const float CloseRange = 1.25f;
    const int MaxVoices = 12;
    static GameSfx current;
    readonly Dictionary<Cue, Sound> sounds = new Dictionary<Cue, Sound>();
    readonly List<AudioSource> voices = new List<AudioSource>(MaxVoices);
    readonly List<AudioSource> flames = new List<AudioSource>();
    readonly System.Random variation = new System.Random();
    Transform listener;
    AudioSource spikeVoice;
    int spikeFrame = -1;

    static GameSfx GetPlayer()
    {
        if (current == null)
            current = new GameObject("Gameplay audio").AddComponent<GameSfx>();
        return current;
    }

    void Awake()
    {
        GameObject snail = GameObject.FindGameObjectWithTag("Player");
        listener = snail != null ? snail.transform : Camera.main?.transform;
        var clips = new Dictionary<string, AudioClip>();
        foreach (var clip in Resources.LoadAll<AudioClip>("SnailSfx"))
            clips[clip.name] = clip;

        Add(Cue.KeyPickup, .74f, 9f);
        Add(Cue.DoorUnlock, .70f, 8f);
        Add(Cue.CabbageBite, .60f, 7f);
        Add(Cue.SnailMove, .25f, 4f);
        Add(Cue.LadybugMove, .22f, 5f);
        Add(Cue.LadybugAttack, .78f, 8f);
        Add(Cue.SpiderMove, .28f, 6f);
        Add(Cue.SpiderAttack, .78f, 9f);
        Add(Cue.FrogSpit, .62f, 9f);
        Add(Cue.SpikeRise, .44f, 6.5f);
        Add(Cue.FlameBurn, .43f, 6f);

        void Add(Cue cue, float gain, float range)
        {
            string name = cue.ToString();
            clips.TryGetValue(name, out AudioClip clip);
            clips.TryGetValue(name + "Alt", out AudioClip alternate);
            sounds.Add(cue, new Sound { clip = clip, alternate = alternate, gain = gain, range = range });
        }
    }

    float Mix(Vector2 position, Sound sound, out float pan)
    {
        Vector2 offset = listener != null ? position - (Vector2)listener.position : Vector2.zero;
        pan = Mathf.Clamp(offset.x / sound.range, -1f, 1f) * .72f;
        float distance = Mathf.InverseLerp(CloseRange, sound.range, offset.magnitude);
        return sound.gain * (1f - Mathf.SmoothStep(0f, 1f, distance));
    }

    public static void Play(Cue cue, Vector3 position)
    {
        GameSfx player = GetPlayer();
        Sound sound = player.sounds[cue];
        float gain = player.Mix(position, sound, out float pan);
        if (sound.clip == null || gain < .01f) return;

        // A row of spikes often emerges on exactly the same frame. Let the
        // nearest spike represent that row, regardless of script update order.
        if (cue == Cue.SpikeRise && player.spikeFrame == Time.frameCount && player.spikeVoice != null)
        {
            if (player.spikeVoice.volume < gain)
            {
                player.spikeVoice.volume = gain;
                player.spikeVoice.panStereo = pan;
            }
            return;
        }

        AudioSource voice = player.GetVoice(gain);
        if (voice == null) return;
        voice.clip = sound.useAlternate && sound.alternate != null ? sound.alternate : sound.clip;
        sound.useAlternate = !sound.useAlternate;
        player.sounds[cue] = sound;
        voice.pitch = .96f + .08f * (float)player.variation.NextDouble();
        voice.volume = gain;
        voice.panStereo = pan;
        voice.Play();
        if (cue == Cue.SpikeRise)
        {
            player.spikeFrame = Time.frameCount;
            player.spikeVoice = voice;
        }
    }

    AudioSource GetVoice(float gain)
    {
        AudioSource quietest = null;
        foreach (AudioSource voice in voices)
        {
            if (!voice.isPlaying) return voice;
            if (quietest == null || voice.volume < quietest.volume) quietest = voice;
        }
        if (voices.Count < MaxVoices)
        {
            var voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voices.Add(voice);
            return voice;
        }
        if (quietest.volume >= gain) return null;
        quietest.Stop();
        if (quietest == spikeVoice) spikeVoice = null;
        return quietest;
    }

    public static void SetFlameLoop(ref AudioSource source, Transform emitter, bool active)
    {
        if (!active)
        {
            if (source != null)
            {
                source.Stop();
                if (current != null) current.flames.Remove(source);
            }
            return;
        }
        GameSfx player = GetPlayer();
        Sound sound = player.sounds[Cue.FlameBurn];
        if (sound.clip == null) return;
        if (source == null)
        {
            source = emitter.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.clip = sound.clip;
        }
        if (source.isPlaying) return;
        source.volume = 0f;
        source.timeSamples = player.variation.Next(sound.clip.samples);
        source.Play();
        player.flames.Add(source);
    }

    void LateUpdate()
    {
        var flame = sounds[Cue.FlameBurn];
        for (int i = flames.Count - 1; i >= 0; --i)
        {
            AudioSource source = flames[i];
            if (source == null)
            {
                flames.RemoveAt(i);
                continue;
            }
            float gain = Mix(source.transform.position, flame, out float pan);
            source.volume = Mathf.MoveTowards(source.volume, gain, Time.unscaledDeltaTime * 4f);
            source.panStereo = pan;
        }
    }
}
