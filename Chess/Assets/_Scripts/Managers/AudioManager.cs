using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource _source;
    public AudioClip PieceMovementClip;
    public AudioClip CheckClip;
    public AudioClip CheckMateClip;
    public float MinimumMovementSoundPitch = 0.9f;
    public float MaximumMovementSoundPitch = 1.1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayAudioClip(AudioClip clip, float minPitch = 1f, float maxPitch = 1f, float volume = 1f)
    {
        _source.pitch = Random.Range(minPitch, maxPitch);
        _source.PlayOneShot(clip);
        _source.pitch = 1f;
    }
}
