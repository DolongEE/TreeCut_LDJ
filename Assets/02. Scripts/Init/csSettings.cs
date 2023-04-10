using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AudioSource))]
public class csSettings : MonoBehaviour
{
    public static csSettings instance;

    public float soundVolume = 1.0f;
    public bool isSoundMute = false;
    public Slider sl;
    public Toggle tg;
    public GameObject Sound;
    public GameObject PlaySoundBtn;

    private AudioSource myaudio;
    public AudioClip[] soundFile;
    public AudioClip[] soundEffect;

    private bool volumechange = false;
    private float tempvolume;
    private bool tempmute;

    GameObject _soundWalk;

    void Awake()
    {
        //싱글톤 생성
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        myaudio = GetComponent<AudioSource>();
        //씬 scLobby이동
        SceneManager.LoadSceneAsync("scLobby");

        //맵마다 배경음 변경
        PlayBackground(0);
    }

    void Start()
    {
        LoadData();

        //볼륨은 값대로 
        soundVolume = sl.value;
        //토글이 isOn일때 뮤트
        isSoundMute = tg.isOn; 

        PlaySoundBtn.SetActive(true);
        AudioSet();
    }

    private void Update()
    {
        if (volumechange)
        {
            volumechange = false;
            AudioTempSet();
        }

        // 핸드폰으로 실행 시켰을때
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
                return;
            }
        }

    }

    // 사운드 설정
    public void SetSound()
    {
        soundVolume = sl.value;
        isSoundMute = tg.isOn;

        tempvolume = soundVolume;
        tempmute = isSoundMute;

        AudioSet();

        OnClickBtnSound();

        Sound.SetActive(false);
        PlaySoundBtn.SetActive(true);
    }

    void AudioSet()
    {
        myaudio.volume = soundVolume;
        myaudio.mute = isSoundMute;
        SaveData();
    }

    void AudioTempSet()
    {
        myaudio.volume = sl.value;
        myaudio.mute = tg.isOn;
    }

    public void SoundUiOpen()
    {
        OnClickBtnSound();

        Sound.SetActive(true);
        PlaySoundBtn.SetActive(false);

        tempvolume = soundVolume;
        tempmute = isSoundMute;
    }

    public void SoundUiclose()
    {
        Sound.SetActive(false);
        PlaySoundBtn.SetActive(true);

        soundVolume = tempvolume;
        isSoundMute = tempmute;

        sl.value = soundVolume;
        tg.isOn = isSoundMute;

        OnClickBtnSound();
    }

    public void OnChangeVolum()
    {
        volumechange = true;
    }

    public void PlayBackground(int stage)
    {
        myaudio.clip = soundFile[stage];
        AudioSet();
        myaudio.Play();
    }


    //플레이 이펙트사운드
    public void PlayEffct(Vector3 pos, int sfx, bool _loop)
    {
        if (isSoundMute)
        {
            return;
        }

        if (sfx != 2)
        {
            GameObject _soundObj = new GameObject("sfx");
            _soundObj.transform.position = pos;
            AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();

            _audioSource.clip = soundEffect[sfx];
            _audioSource.volume = soundVolume;
            _audioSource.minDistance = 15.0f;
            _audioSource.maxDistance = 30.0f;
            _audioSource.loop = _loop;
            _audioSource.Play();

            Destroy(_soundObj, _audioSource.clip.length);
        }
    }

    //플레이어 걸음 사운드
    public void PlayWalk()
    {
        AudioSource _audioSource;

        if (_soundWalk == null)
        {
            _soundWalk = new GameObject("walk_sound");
            _soundWalk.transform.position = this.transform.position;
            _audioSource = _soundWalk.AddComponent<AudioSource>();
        }
        else
        {
            _audioSource = _soundWalk.GetComponent<AudioSource>();
        }

        _audioSource.Stop();
        _audioSource.clip = soundEffect[0];
        _audioSource.volume = soundVolume;
        _audioSource.minDistance = 15.0f;
        _audioSource.maxDistance = 30.0f;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void StopWalk()
    {
        if (_soundWalk == null)
        {
            return;
        }

        AudioSource _audioSource = _soundWalk.GetComponent<AudioSource>();
        _audioSource.Stop();
    }

    //클릭 사운드
    public void OnClickBtnSound()
    {
        if (isSoundMute)
        {
            return;
        }

        GameObject _soundObj = new GameObject("sfx");
        AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();

        _audioSource.clip = soundEffect[2];
        _audioSource.volume = soundVolume;
        _audioSource.Play();

        Destroy(_soundObj, soundEffect[0].length);
    }

    //버튼 사운드
    IEnumerator BtnSounds()
    {
        GameObject _soundObj = new GameObject("sfx");
        AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();

        _audioSource.clip = soundEffect[2];
        _audioSource.volume = soundVolume;
        _audioSource.Play();

        Destroy(_soundObj, soundEffect[2].length);

        yield return null;
    }

    //사운드 데이터 저장
    public void SaveData()
    {
        PlayerPrefs.SetFloat("SOUNDVOLUME", soundVolume);
        PlayerPrefs.SetInt("ISSOUNDMUTE", System.Convert.ToInt32(isSoundMute));
    }

    //플레이어 정보 불러오기
    public void LoadData()
    {
        sl.value = PlayerPrefs.GetFloat("SOUNDVOLUME");
        tg.isOn = System.Convert.ToBoolean(PlayerPrefs.GetInt("ISSOUNDMUTE"));

        int isSave = PlayerPrefs.GetInt("ISSAVE");
        if (isSave == 0)
        {
            sl.value = 0.5f;
            tg.isOn = false;

            SaveData();
            PlayerPrefs.SetInt("ISSAVE", 1);
        }
    }


}
