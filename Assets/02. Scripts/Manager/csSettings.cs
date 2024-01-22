using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AudioSource))]
public class csSettings : MonoBehaviour
{
    public static csSettings instance;

    //버튼 관련
    public Slider sliderSound;
    public Toggle toggleMute;
    public GameObject pnlSound;
    public GameObject btnSetting;

    //소리 관련
    private AudioSource myAudio;
    public AudioClip[] soundFile;
    public AudioClip[] soundEffect;
        
    private float soundVolume;
    private bool isSoundMute;

    private float tempVolume;
    private bool tempMute;

    private GameObject _soundWalk;

    void Awake()
    {
        //싱글톤
        instance = this;

        DontDestroyOnLoad(gameObject);
        myAudio = GetComponent<AudioSource>();
        SceneManager.LoadSceneAsync("scLobby");        
    }

    void Start()
    {
        //저장된 사운드 값 불러옴
        LoadData();

        //배경음
        myAudio.clip = soundFile[0];
        myAudio.Play();

        //볼륨은 값대로
        soundVolume = sliderSound.value;

        //토글이 isOn일때 뮤트
        isSoundMute = toggleMute.isOn; 

        btnSetting.SetActive(true);

        AudioSet();
    }

    private void Update()
    {
        // 핸드폰 종료
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
                return;
            }
        }
    }

    //사운드 설정
    public void SetSound()
    {
        soundVolume = sliderSound.value;
        isSoundMute = toggleMute.isOn;

        tempVolume = soundVolume;
        tempMute = isSoundMute;

        AudioSet();

        BtnSound();

        pnlSound.SetActive(false);
        btnSetting.SetActive(true);
    }

    void AudioSet()
    {
        myAudio.volume = soundVolume;
        myAudio.mute = isSoundMute;

        SaveData();
    }

    public void SoundUiOpen()
    {
        BtnSound();

        pnlSound.SetActive(true);
        btnSetting.SetActive(false);

        tempVolume = soundVolume;
        tempMute = isSoundMute;
    }

    public void SoundUiclose()
    {
        pnlSound.SetActive(false);
        btnSetting.SetActive(true);

        soundVolume = tempVolume;
        isSoundMute = tempMute;

        sliderSound.value = soundVolume;
        toggleMute.isOn = isSoundMute;

        BtnSound();
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

        GameObject clickSfx = new GameObject("clickSfx");
        AudioSource _audioSource = clickSfx.AddComponent<AudioSource>();

        _audioSource.clip = soundEffect[2];
        _audioSource.volume = soundVolume;
        _audioSource.Play();

        Destroy(clickSfx, soundEffect[0].length);
    }

    //버튼 사운드
    IEnumerator BtnSounds()
    {
        GameObject _soundObj = new GameObject("btnSfx");
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
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
        PlayerPrefs.SetInt("SoundMute", System.Convert.ToInt32(isSoundMute));
    }

    //저장된 사운드 데이터 불러오기
    public void LoadData()
    {
        sliderSound.value = PlayerPrefs.GetFloat("SoundVolume");
        toggleMute.isOn = System.Convert.ToBoolean(PlayerPrefs.GetInt("SoundMute"));

        int isSave = PlayerPrefs.GetInt("ISSAVE");

        if (isSave == 0)
        {
            sliderSound.value = 0.5f;
            toggleMute.isOn = false;

            SaveData();
            PlayerPrefs.SetInt("ISSAVE", 1);
        }
    }
}
