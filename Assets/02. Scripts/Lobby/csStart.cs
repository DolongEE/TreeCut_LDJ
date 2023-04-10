using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class csStart : MonoBehaviour
{
    public GameObject lodding_panel;
    public Button start_btn;

    private void Start()
    {
        lodding_panel.SetActive(false);
    }

    // 게임 씬으로 이동
    public void OnClickStartBtn()
    {
        csSettings.instance.OnClickBtnSound();

        lodding_panel.SetActive(true);
        start_btn.interactable = false;

        //비동기 방식
        SceneManager.LoadSceneAsync("scMine");
    }
}
