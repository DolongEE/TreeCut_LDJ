using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csMoveItem : MonoBehaviour {

    public Transform target;
    private Transform tr;
    
    private SpriteRenderer sp;

    private float speed = 0.1f;

    private bool delobj = false;

    public enum TYPE { wood = 0 };
    public TYPE item_type;

    private void Start()
    {
        //아이템 타입이 wood일때 UI아이템 위치로 이동
        if(item_type == TYPE.wood)
        {
            target = GameObject.FindGameObjectWithTag("wood_pos").transform;
        }

        tr = GetComponent<Transform>();
        sp = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(tr.position!= target.position)
        {
            transform.position = Vector3.MoveTowards(tr.position, target.position, speed * Time.deltaTime);
            speed += 0.1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //wood아이템 정보 추가
        if(col.gameObject.tag == "wood_pos" && item_type == TYPE.wood)
        {
            if (!delobj)
            {
                delobj = true;

                StartCoroutine(StatEffectSound());

                sp.color = new Color(0, 0, 0, 0);

                if (item_type == TYPE.wood)
                {
                    csItemManager.instance.SetScore(0);
                }                
                Invoke("DestroyObj", 1.0f);
            }
        }
    }

    IEnumerator StatEffectSound()
    {
        csSettings.instance.PlayEffect(transform.position, 1, false);
        yield return new WaitForSeconds(0.3f);
    }

    void DestroyObj()
    {
        Destroy(gameObject);
    }
}
