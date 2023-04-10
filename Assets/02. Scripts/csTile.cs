using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//타일 위치값 설정
[System.Serializable]
public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class csTile : MonoBehaviour
{
    public bool tileMove = false;

    public Point tilePos;

    public float f;
    public float g;
    public float h;

    public int nodeID;
    public bool nodecheck = false;

    public csTile parentTile;

    public SpriteRenderer sp;

    [HideInInspector]
    public csTree tree;
    public bool havetree = false;

    [HideInInspector]
    public bool target = false;

    private void Awake()
    {
        parentTile = GetComponent<csTile>();
    }

    public void SetPos(int y, int x, int num)
    {
        tilePos.x = x;
        tilePos.y = y;
        nodeID = num;
    }

    public void SetColor()
    {
        nodecheck = true;
        sp.color = new Color(0.8f, 1, 1, 1);
    }

    private void Update()
    {
        if (!nodecheck && sp.color != new Color(1, 1, 1, 1))
        {
            sp.color = new Color(1, 1, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {

            sp.color = new Color(1, 1, 1, 1);
        }

        if (col.gameObject.tag != "targetcheck" && havetree == true)
        {
            target = true;
            tree.isActiveFalse();
        }
    }
}
