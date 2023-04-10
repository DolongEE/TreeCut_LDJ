using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class csSetTarget : MonoBehaviour
{
    public bool check = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        //타일태그가 CanMove시 이동
        if (col.gameObject.tag == "CanMove" && check == false)
        {
            check = true;

            csTile temp = col.gameObject.GetComponent<csTile>();

            if (temp.havetree)
            {
                temp.tree.isActiveTrue();
                csPlayerCtrl.instance.OnNode(temp, true);
            }
            else
            {
                csPlayerCtrl.instance.OnNode(temp, false);
            }
        }
    }
}
