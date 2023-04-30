using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AudioSource))]
public class csSettings : MonoBehaviour
{
    public static csSettings instance;

    public Slider slider;
    public Toggle toggle;
    public GameObject Sound;
    public GameObject PlaySoundBtn;

    private AudioSource myaudio;
    public AudioClip[] soundFile;
    public AudioClip[] soundEffect;

    private bool volumeChange = false;
    private float soundVolume;
    private bool isSoundMute;

    private float tempVolume;
    private bool tempMute;

    GameObject _soundWalk;

    void Awake()
    {
        //싱글톤
        instance = this;

        DontDestroyOnLoad(this.gameObject);
        myaudio = GetComponent<AudioSource>();
        SceneManager.LoadSceneAsync("scLobby");        
    }

    void Start()
    {
        LoadData();

        //배경음
        myaudio.clip = soundFile[0];
        myaudio.Play();
        
        //볼륨은 값대로
        soundVolume = slider.value;
        //토글이 isOn일때 뮤트
        isSoundMute = toggle.isOn; 

        PlaySoundBtn.SetActive(true);

        AudioSet();
    }

    private void Update()
    {
        if (volumeChange)
        {
            volumeChange = false;
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
        soundVolume = slider.value;
        isSoundMute = toggle.isOn;

        tempVolume = soundVolume;
        tempMute = isSoundMute;

        AudioSet();

        BtnSound();

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
        myaudio.volume = slider.value;
        myaudio.mute = toggle.isOn;
    }

    public void SoundUiOpen()
    {
        BtnSound();

        Sound.SetActive(true);
        PlaySoundBtn.SetActive(false);

        tempVolume = soundVolume;
        tempMute = isSoundMute;
    }

    public void SoundUiclose()
    {
        Sound.SetActive(false);
        PlaySoundBtn.SetActive(true);

        soundVolume = tempVolume;
        isSoundMute = tempMute;

        slider.value = soundVolume;
        toggle.isOn = isSoundMute;

        BtnSound();
    }

    public void OnChangeVolum()
    {
        volumeChange = true;
    }

    //플레이 이펙트사운드
    public void PlayEffect(Vector3 pos, int sfx, bool isLoop)
    {
        if (isSoundMute)
        {
            return;
        }

        if (sfx != 2)
        {
            GameObject effectSfx = new GameObject("sfx");
            effectSfx.transform.position = pos;
            AudioSource _audioSource = effectSfx.AddComponent<AudioSource>();

            _audioSource.clip = soundEffect[sfx];
            _audioSource.volume = soundVolume;
            _audioSource.minDistance = 15.0f;
            _audioSource.maxDistance = 30.0f;
            _audioSource.loop = isLoop;
            _audioSource.Play();

            Destroy(effectSfx, _audioSource.clip.length);
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
    public void BtnSound()
    {
        if (isSoundMute)
        {
            return;
        }

        GameObject clickSfx = new GameObject("ClickSfx");
        AudioSource _audioSource = clickSfx.AddComponent<AudioSource>();

        _audioSource.clip = soundEffect[2];
        _audioSource.volume = soundVolume;
        _audioSource.Play();

        Destroy(clickSfx, soundEffect[0].length);
    }

    //버튼 사운드
    IEnumerator BtnSounds()
    {
        GameObject _soundObj = new GameObject("BtnSfx");
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
        Debug.Log(soundVolume + "볼륨 저장");
        Debug.Log(System.Convert.ToInt32(isSoundMute) + "음소거 저장");
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
        PlayerPrefs.SetInt("SoundMute", System.Convert.ToInt32(isSoundMute));
    }

    //플레이어 정보 불러오기
    public void LoadData()
    {
        Debug.Log(PlayerPrefs.GetFloat("SoundVolume") + "볼륨 불러옴");
        slider.value = PlayerPrefs.GetFloat("SoundVolume");
        toggle.isOn = System.Convert.ToBoolean(PlayerPrefs.GetInt("SoundMute"));

        int isSave = PlayerPrefs.GetInt("ISSAVE");

        if (isSave == 0)
        {
            slider.value = 0.5f;
            toggle.isOn = false;

            SaveData();
            PlayerPrefs.SetInt("ISSAVE", 1);
        }
    }


}
