using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csItemManager : MonoBehaviour
{
    public static csItemManager instance = null;

    public int cost = 0;
    public Text txtWoodShadow;
    public Text txtWood;
    private const string costX = "X ";

    void Awake()
    {
        instance = this;
    }

    //아이템 정보 저장
    public void SetScore(int cost_type)
    {
        if (cost_type == 0)
        {
            cost++;
            txtWoodShadow.text = costX + cost;
            txtWood.text = txtWoodShadow.text;
        }
        csInitData.instance.SavePlayerData(cost);
    }

    //아이템 정보 불러오기
    public void LoadScore()
    {
        cost = csInitData.instance.myData.wood;

        txtWoodShadow.text = "X " + cost;
        txtWood.text = txtWoodShadow.text;
    }
}
