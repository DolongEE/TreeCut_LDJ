using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csTree : MonoBehaviour
{
    public float x;
    public float y;

    public csTile pos;

    public int max_hp;
    public int hp;
    public bool die = false;

    public int cost;

    public Sprite normalIMG;
    public Sprite activeIMG;

    public bool active = false;

    private SpriteRenderer sp;

    private GameObject hp_bar;
    private csFollowHPbar hp_bar_ui;

    private csPlayerCtrl playerCtrl;

    private float _Alpha = 0;


    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        hp_bar_ui = GameObject.FindGameObjectWithTag("HP_BAR_UI").GetComponent<csFollowHPbar>();
        playerCtrl = csPlayerCtrl.instance;
    }

    private void Start()
    {
        //hp바 생성
        hp_bar = Instantiate(csInitData.instance.hpbar, this.gameObject.transform);
        hp_bar.SetActive(false);
        max_hp = hp;
    }

    private void Update()
    {
        if (die)
        {
            sp.sprite = activeIMG;
            return;
        }

        if (active)
        {
            Point targetPos = playerCtrl.endPos.tilePos;

            if (targetPos.x != pos.tilePos.x && targetPos.y != pos.tilePos.y)
            {
                isActiveFalse();
            }
        }

        if ((playerCtrl.playerPos.x == pos.tilePos.x || playerCtrl.playerPos.x - 1 == pos.tilePos.x || playerCtrl.playerPos.x + 1 == pos.tilePos.x) && (playerCtrl.playerPos.y + 1 == pos.tilePos.y || playerCtrl.playerPos.y + 2 == pos.tilePos.y))
        {
            if (_Alpha != 0.5f)
            {
                _Alpha = 0.5f;
                sp.color = new Color(1, 1, 1, _Alpha);
            }
        }
        else
        {
            if (_Alpha != 1.0f)
            {
                _Alpha = 1.0f;

                sp.color = new Color(1, 1, 1, _Alpha);
            }
        }
    }

    public void isActiveFalse()
    {
        hp_bar.SetActive(false);
        active = false;
        this.transform.localScale = new Vector3(0.3f, 0.3f, 1);
        sp.sprite = normalIMG;
    }

    public void isActiveTrue()
    {
        hp_bar_ui.hp_count.text = "" + hp;
        hp_bar_ui.hp_fill.fillAmount = (float)hp / (float)max_hp;
        Debug.Log(hp_bar_ui.hp_fill.fillAmount);
        hp_bar.SetActive(true);
        active = true;
        //Debug.Log(active);
        this.transform.localScale = new Vector3(1.2f, 1.2f, 1);
        sp.sprite = activeIMG;
    }

    //공격당했을때
    public void Hit()
    {
        hp -= csLevelManager.instance.player_dmg;

        Debug.Log("TREE HP : " + hp);

        if (hp <= 0)
        {
            StartCoroutine(IsDie());
        }
        else
        {
            //StopCoroutine(StartHit());
            StartCoroutine(StartHit());
        }
    }

    IEnumerator StartHit()
    {
        float temp = 0;

        yield return new WaitForSeconds(0.3f);

        hp_bar_ui.hp_count.text = "" + hp;
        hp_bar_ui.hp_fill.fillAmount = (float)hp / (float)max_hp;

        while (temp != 0)
        {
            if (temp > 0)
            {
                temp -= 0.2f;
            }
            else if (temp <= 0)
            {
                temp = 0;
            }
            //Debug.Log(sp.color);
            sp.color = new Color(1, temp, temp, _Alpha);
            yield return new WaitForSeconds(0.1f);
        }

        while (temp != 1)
        {
            if (temp < 1)
            {
                temp += 0.2f;
            }
            else if (temp >= 1)
            {
                temp = 1;
            }
            //Debug.Log(sp.color);
            sp.color = new Color(1, temp, temp, _Alpha);
            yield return new WaitForSeconds(0.1f);
        }
    }

    //HP가 0일때
    IEnumerator IsDie()
    {
        float temp = 1;

        playerCtrl.canAx = false;
        csLevelManager.instance.BtnOff();

        sp.sprite = activeIMG;

        csSettings.instance.PlayEffct(this.transform.position, Random.Range(3, 5), false);

        pos.tileMove = true;
        pos.havetree = false;

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(CreateCost());

        hp_bar.SetActive(false);

        while (temp != 0)
        {
            if (temp > 0)
            {
                temp -= 0.4f;
            }
            else if (temp <= 0)
            {
                temp = 0;
            }

            sp.color = new Color(1, 1, 1, temp);
            yield return new WaitForSeconds(0.2f);
        }

        Destroy(this.gameObject);
    }

    //아이템 생성
    IEnumerator CreateCost()
    {
        for (int i = 0; i < cost; i++)
        {
            GameObject tempobj = Instantiate(csInitData.instance.wood, null);
            tempobj.transform.position = this.transform.position;
            tempobj.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-30, 30), Random.Range(50, 150)));

            yield return new WaitForSeconds(0.1f);
        }
    }
}
