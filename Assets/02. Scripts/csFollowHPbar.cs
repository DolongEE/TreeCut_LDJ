using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csFollowHPbar : MonoBehaviour
{
    public Text hp_count;

    private Transform target;

    public Image hp_bar;
    public Image hp_fill;

    void Start()
    {
        hp_count.text = "";
        hp_bar.enabled = false;
        hp_fill.enabled = false;
    }

    //선택한 나무 HP바 생성
    private void Update()
    {
        GameObject tempObj = GameObject.FindGameObjectWithTag("HP_BAR");

        if (tempObj != null)
        {
            hp_bar.enabled = true;
            hp_fill.enabled = true;

            target = tempObj.GetComponent<Transform>();

            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

            this.transform.position = screenPos;
            hp_count.transform.position = screenPos;
        }
        else
        {
            hp_bar.enabled = false;
            hp_fill.enabled = false;
            hp_count.text = "";
        }
    }
}
