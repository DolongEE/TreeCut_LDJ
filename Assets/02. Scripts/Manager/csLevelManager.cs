using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//플레이어 공격 관련 스크립트
public class csLevelManager : MonoBehaviour
{
    public static csLevelManager instance;
    
    public Button btnAttack;
    public Text txtAttack;

    private Image btnImage;
    private Sprite btntrue;
    private Sprite btnfalse;

    public int playerDmg;
    public int dmgUpCost;

    public Button btnDmgUp;
    public Text txtDmgUpCost;

    private void Awake()
    {
        instance = this;
        btnImage = btnAttack.GetComponent<Image>();

        SetPlayerDmg();
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
        if (dmgUpCost <= csInitData.instance.myData.wood)
        {
            btnDmgUp.interactable = true;
        }
        else
        {
            btnDmgUp.interactable = false;
        }
    }

    //데미지 설정
    void SetPlayerDmg()
    {
        playerDmg = csInitData.instance.myData.dmg;

        dmgUpCost = playerDmg * 10;
        txtDmgUpCost.text = "현재 데미지 : " + playerDmg + "\n(나무 " + dmgUpCost.ToString() + " 필요)";
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
    public void OnClickDmgUp()
    {
        csInitData.instance.UpgradePlayerDmg();

        SetPlayerDmg();

        csItemManager.instance.LoadScore();
    }
}
