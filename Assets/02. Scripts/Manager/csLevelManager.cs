using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csLevelManager : MonoBehaviour
{
    public static csLevelManager instance;

    public Button btnAttack;
    public Text txtAttack;

    private Image btnImage;
    private Sprite btntrue;
    private Sprite btnfalse;

    public int player_dmg;
    public int dmg_up_cost;

    public Button dmg_up_btn;
    public Text dmg_up_cost_text;

    private void Awake()
    {
        instance = this;
        btnImage = btnAttack.GetComponent<Image>();

        SetPlayerDMG();
    }

    private void Start()
    {
        btntrue = csInitData.instance.btntrue;
        btnfalse = csInitData.instance.btnfalse;

        btnAttack.interactable = false;
        btnImage.sprite = btnfalse;
    }

    private void Update()
    {
        //나무가 충분히 모이면 데미지 업그레이드 가능
        if (dmg_up_cost <= csInitData.instance.myData.wood)
        {
            dmg_up_btn.interactable = true;
        }
        else
        {
            dmg_up_btn.interactable = false;
        }
    }

    //데미지 설정
    void SetPlayerDMG()
    {
        player_dmg = csInitData.instance.myData.dmg;

        dmg_up_cost = player_dmg * 10;
        dmg_up_cost_text.text = "현재 데미지 : " + player_dmg + "\n(나무 " + dmg_up_cost.ToString() + " 필요)";
    }

    public void BtnOn()
    {
        btnAttack.interactable = true;

        btnImage.sprite = btntrue;
    }

    public void BtnOff()
    {
        btnAttack.interactable = false;

        btnImage.sprite = btnfalse;
    }

    //데미지 상승 버튼
    public void OnClickDMGUpBtn()
    {
        csInitData.instance.UpgradePlayerDmg();

        SetPlayerDMG();

        csPlayerCtrl.instance.LoadScore();
    }
}
