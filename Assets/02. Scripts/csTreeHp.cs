using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//나무 HP생성 스크립트
public class csTreeHp : MonoBehaviour
{
    public Text hp_count;

    private Transform target;

    public Image hp_bar;
    public Image hp_max;

    void Start()
    {
        hp_count.text = "";
        hp_bar.enabled = false;
        hp_max.enabled = false;
    }

    //선택한 나무 HP바 생성
    private void Update()
    {
        GameObject tempObj = GameObject.FindGameObjectWithTag("HP_BAR");

        if (tempObj != null)
        {
            hp_bar.enabled = true;
            hp_max.enabled = true;

            target = tempObj.GetComponent<Transform>();

            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);

            this.transform.position = screenPos;
            hp_count.transform.position = screenPos;
        }
        else
        {
            hp_bar.enabled = false;
            hp_max.enabled = false;
            hp_count.text = "";
        }
    }
}
