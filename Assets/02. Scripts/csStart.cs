using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class csStart : MonoBehaviour
{
    public GameObject pnlLoading;
    public Button start_btn;

    private void Start()
    {
        pnlLoading.SetActive(false);
    }

    // 게임 씬으로 이동
    public void OnClickStartBtn()
    {
        csSettings.instance.BtnSound();

        pnlLoading.SetActive(true);
        start_btn.interactable = false;

        //비동기 방식
        SceneManager.LoadSceneAsync("scTree");
    }
}
