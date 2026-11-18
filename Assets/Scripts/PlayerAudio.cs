using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip damageClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.3f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    public void PlayJump() => Play(jumpClip);
    public void PlayDamage() => Play(damageClip);

    private void Play(AudioClip clip)
    {
        if (clip != null)
            source.PlayOneShot(clip, volume);
    }
}
