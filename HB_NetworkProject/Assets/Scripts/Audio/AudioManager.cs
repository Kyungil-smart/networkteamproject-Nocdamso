using UnityEngine;

public enum SoundType 
{ 
    Select,     // 기물 선택
    Move,       // 기물 이동
    Capture,    // 기물 잡기
    Check,      // 체크 발생
    Victory,    // 승리
    Defeat      // 패배
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("재생 소스")]
    [SerializeField] private AudioSource _effectsSource;

    [Header("효과음 리스트")]
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip captureSound;
    [SerializeField] private AudioClip checkSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }
    }

    public void Play(SoundType type)
    {
        AudioClip clip = GetClip(type);

        if (clip != null && _effectsSource != null)
        {
            _effectsSource.PlayOneShot(clip);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && _effectsSource != null)
        {
            _effectsSource.PlayOneShot(clip);
        }
    }

    private AudioClip GetClip(SoundType type)
    {
        return type switch
        {
            SoundType.Select => selectSound,
            SoundType.Move => moveSound,
            SoundType.Capture => captureSound,
            SoundType.Check => checkSound,
            SoundType.Victory => victorySound,
            SoundType.Defeat => defeatSound,
            _ => null
        };
    }
}
