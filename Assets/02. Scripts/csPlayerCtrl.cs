using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

//플레이어 방향 설정
[System.Serializable]
public struct Direction
{
    public Point pos;
    public float g;

    public Direction(Point _pos, float _g)
    {
        this.pos = _pos;
        this.g = _g;
    }

    public Direction(int x, int y, float _g)
    {
        this.pos.x = x;
        this.pos.y = y;
        this.g = _g;
    }
}

public class csPlayerCtrl : MonoBehaviour
{
    [HideInInspector]
    public static csPlayerCtrl instance;

    [HideInInspector]
    public csTile[,] tiles;

    [HideInInspector]
    public Point playerPos;
    private bool playerMoveCenter = false;

    private bool playerMove = false;

    public csTile startPos { get; set; }
    public csTile endPos { get; set; }
    public csTile beforendPos { get; set; }
    public csTile playerNode { get; set; }
    private csTile lastPos = null;

    //A*알고리즘 적용
    private List<csTile> openList;
    private List<csTile> closeList;
    private List<csTile> nodeList;

    public List<csTile> NodeList
    {
        get { return nodeList; }
        set { nodeList = value; }
    }


    private Direction[] direction = {
        new Direction(1, 0, 10f), new Direction(-1, 0, 10f),
        new Direction(0, 1, 10f), new Direction(0, -1, 10f)
    };

    private Animator anim;

    private SpriteRenderer sp;

    [HideInInspector]
    public int cost = 0;

    private bool treebool = false;

    private GameObject targetObj = null;
    private csSetTarget cstarget;

    private bool stopcor = false;

    [HideInInspector]
    public bool canAx = false;
    private bool onbtn = false;

    private int looktree = 0;
    private bool playhitanim = false;

    public Text wood_text;
    public Text woods_text;
    private const string costX = "X ";

    private void Awake()
    {
        instance = this;

        tiles = new csTile[87, 50];

        openList = new List<csTile>();
        closeList = new List<csTile>();
        nodeList = new List<csTile>();

        anim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        LoadScore();

        playerNode = tiles[50, 19];
        playerPos = playerNode.tilePos;
        beforendPos = playerNode;
        this.transform.position = new Vector3(-15 + (playerPos.x * 0.9f), 48.3f - (playerPos.y * 0.9f) + 0.5f, 0);

        targetObj = Instantiate(csInitData.instance.target, this.transform);
        cstarget = targetObj.GetComponent<csSetTarget>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        //마우스 포인터 위치
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() == true)
            {
                return;
            }

            Vector2 curser = Input.mousePosition;
            curser = Camera.main.ScreenToWorldPoint(curser);
            //Debug.Log(curser);

            targetObj.transform.position = curser;
            cstarget.check = false;

        }
        //핸드폰 터치 시
#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (!IsPointerOverUIObject(touch.position))
            {//EventSystem.current.IsPointerOverGameObject (i) == false

                if (touch.phase == TouchPhase.Ended)
                {
                    Vector2 curser = touch.position;
                    curser = Camera.main.ScreenToWorldPoint(curser);
                    //Debug.Log(curser);

                    //if (targetObj == null)
                    {
                        targetObj = Instantiate(csInitData.instance.target);
                        targetObj.transform.position = curser;
                    }
                }
            }
        }
#endif

        if (playerMoveCenter && !playerMove)
        {
            playerMoveCenter = false;
            //Debug.Log(playerPos.x);
            //Debug.Log(playerPos.y);

            if (SetNode())
            {
                //stopcor = false;
                FindNode();
                StartCoroutine(PlayerMove());
            }
        }

        if (onbtn && canAx)
        {
            DoAttack();
        }
    }

    //아이템 정보 저장
    public void SetScore(int cost_type)
    {
        if (cost_type == 0)
        {
            cost++;
            wood_text.text = costX + cost;
            woods_text.text = wood_text.text;
        }
        csInitData.instance.SavePlayerData(cost);
    }

    //아이템 정보 불러오기
    public void LoadScore()
    {
        cost = csInitData.instance.myData.wood;

        wood_text.text = "X " + cost;
        woods_text.text = wood_text.text;
    }

    //핸드폰 조작시 터치위치 감지
    public bool IsPointerOverUIObject(Vector2 touchPos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);

        eventDataCurrentPosition.position = touchPos;

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0;
    }

    //공격키
    private void DoAttack()
    {
        StartCoroutine(AttackSound());

        playhitanim = true;
        StartCoroutine(AttackPlayAnim());

        if (playhitanim)
        {
            if (looktree == 0 || looktree == 1)
            {
                anim.Play("attacks");
            }
            else if (looktree == 2)
            {
                anim.Play("attacku");
            }
            else if (looktree == 3)
            {
                anim.Play("attack");
            }

            if (endPos.havetree)
            {
                endPos.tree.Hit();
            }
        }

        onbtn = false;

        if (!playerMove)
        {
            canAx = false;
        }
    }

    IEnumerator AttackSound()
    {
        csSettings.instance.PlayEffct(this.transform.position, Random.Range(5, 8), false);

        yield return null;
    }

    IEnumerator AttackPlayAnim()
    {
        yield return new WaitForSeconds(0.8f);
        playhitanim = false;
    }

    public void OnButtonCheck()
    {
        if (playhitanim == false)
        {
            onbtn = true;
        }
    }

    //A*알고리즘
    bool SetNode()
    {
        startPos = playerNode;

        //Debug.Log("startpos " + startPos.tilePos.x + " / " + startPos.tilePos.y);
        //Debug.Log("endpos " + endPos.tilePos.x + " / " + endPos.tilePos.y);
        //Debug.Log("playerpos " + playerNode.tilePos.x + " / " + playerNode.tilePos.y);

        csTile myTile = tiles[startPos.tilePos.y, startPos.tilePos.x];

        myTile.g = 0;
        myTile.h = Mathf.Abs(endPos.tilePos.x - startPos.tilePos.x) + Mathf.Abs(endPos.tilePos.y - startPos.tilePos.y);
        myTile.f = myTile.g + myTile.h;

        tiles[startPos.tilePos.y, startPos.tilePos.x] = myTile;

        openList.Add(myTile);

        while (openList.Count > 0)
        {
            csTile temp = openList[0];

            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].f < temp.f)
                {
                    temp = openList[i];
                }
            }

            if (temp.tilePos.x == endPos.tilePos.x && temp.tilePos.y == endPos.tilePos.y)
            {
                //Debug.Log("!!find");
                break;
            }

            openList.Remove(temp);
            closeList.Add(temp);
            NearTile(temp);
        }

        if (openList.Count == 0)
        {
            csSettings.instance.StopWalk();

            Debug.Log("FAIL");
            return false;
        }

        return true;
    }

    //근처 타일 정보
    private void NearTile(csTile tile)
    {
        for (int i = 0; i < direction.Length; i++)
        {
            Point pos = new Point(tile.tilePos.x + direction[i].pos.x, tile.tilePos.y + direction[i].pos.y);

            if (pos.x < 0 || pos.x >= 50 || pos.y < 0 || pos.y >= 87)
            {
                continue;
            }

            csTile temp = tiles[pos.y, pos.x];

            if (closeList.Contains(temp) || temp.tileMove == false)
            {
                continue;
            }

            if (temp.havetree == true)
            {
                if (temp.tree.active == false)
                {
                    continue;
                }

                if (lastPos != null)
                {
                    if (temp.tilePos.x == lastPos.tilePos.x && temp.tilePos.y == lastPos.tilePos.y)
                    {
                        continue;
                    }
                }
            }

            if (!openList.Contains(temp))
            {
                temp.g = tile.g + direction[i].g;
                temp.h = Mathf.Abs(endPos.tilePos.x - pos.x) + Mathf.Abs(endPos.tilePos.y - pos.y);
                temp.f = temp.g + temp.h;
                temp.parentTile = tile;
                tiles[pos.y, pos.x] = temp;
                openList.Add(temp);
            }
            else if (tiles[pos.y, pos.x].g > tile.g + 10)
            {
                temp.g = tile.g + direction[i].g;
                temp.f = temp.g + temp.h;
                temp.parentTile = tile;
            }
        }
    }

    void FindNode()
    {
        // Debug.Log("FindNode");
        csTile temp;

        if (treebool)
        {
            temp = tiles[endPos.tilePos.y, endPos.tilePos.x].parentTile;
        }
        else
        {
            temp = tiles[endPos.tilePos.y, endPos.tilePos.x];
        }

        // Debug.Log("= " + startPos.tilePos.x + " / " + startPos.tilePos.y);
        //Debug.Log("== " + endPos.tilePos.x + " / " + endPos.tilePos.y);
        //Debug.Log("==== " + playerNode.tilePos.x + " / " + playerNode.tilePos.y);

        while (temp != null)
        {
            nodeList.Add(temp);

            if (temp.parentTile != null)
            {

                if (temp.parentTile.Equals(startPos))
                {
                    break;
                }
            }

            if (temp.parentTile == temp)
            {
                break;
            }

            temp = temp.parentTile;
        }
        //nodeList.Reverse();

        //Debug.Log(nodeList.Count);

        for (int i = 0; i < nodeList.Count; i++)
        {
            //Debug.Log(nodeList[i].tilePos.x + "/"+ nodeList[i].tilePos.y);
            nodeList[i].SetColor();
        }

    }

    IEnumerator PlayerMove()
    {
        stopcor = true;
        playerMove = true;

        int num;

        csTile[] tempnodes = new csTile[nodeList.Count];
        csTile player = playerNode;

        float speed = 0.1f;

        bool movenow = true;
        int way = 0;
        int way2 = 0;

        num = nodeList.Count - 1;

        for (int i = 0; i < nodeList.Count; i++)
        {
            tempnodes[i] = nodeList[i];
        }


        csTile temp = tempnodes[num];

        anim.SetFloat("speed", 1.0f);

        if (player.tilePos.x < temp.tilePos.x)
        {
            if (movenow)
            {
                movenow = false;
                anim.Play("walks");
                //anim.SetTrigger("moveside");
            }
        }
        else if (player.tilePos.x > temp.tilePos.x)
        {
            if (movenow)
            {
                movenow = false;
                anim.Play("walks");
                //anim.SetTrigger("moveside");
            }
        }
        else if (player.tilePos.y < temp.tilePos.y)
        {
            if (movenow)
            {
                movenow = false;
                anim.Play("walk");
                //anim.SetTrigger("move");
            }
        }
        else if (player.tilePos.y > temp.tilePos.y)
        {
            if (movenow)
            {
                movenow = false;
                anim.Play("walku");
                //anim.SetTrigger("moveup");
            }
        }

        yield return new WaitForEndOfFrame();
        //Debug.Log("count : " + nodeList.Count);
        //Debug.Log("//////" + player.tilePos.x + "/" + player.tilePos.y);
        // Debug.Log("//////" + temp.tilePos.x + "/" + temp.tilePos.y);
        int limite = 0;
        Vector3 pos = pos = new Vector3(-15 + (temp.tilePos.x * 0.9f), 48.3f - (temp.tilePos.y * 0.9f) + 0.5f, 0); ;

        do
        {
            if (!playerMove)
            {
                break;
            }
            //Debug.Log("surch : " + num);
            pos = new Vector3(-15 + (temp.tilePos.x * 0.9f), 48.3f - (temp.tilePos.y * 0.9f) + 0.5f, 0);

            //Debug.Log("//////" + player.tilePos.x + "/" + player.tilePos.y);
            //Debug.Log("@@@@" + temp.tilePos.x + "/" + temp.tilePos.y);

            if (player.tilePos.x < temp.tilePos.x)
            {
                limite = 0;

                if (movenow)
                {
                    movenow = false;
                    anim.Play("walks");
                    //anim.SetTrigger("moveside");
                    way = 0;
                    yield return new WaitForEndOfFrame();
                }

                if (sp.flipX)
                {
                    sp.flipX = false;
                }

                this.transform.position = new Vector2(this.transform.position.x + speed, this.transform.position.y);

                if (this.transform.position.x > pos.x)
                {
                    this.transform.position = new Vector2(pos.x, this.transform.position.y);
                }
                Debug.Log("MOVE RIGHT");

            }
            else if (player.tilePos.x > temp.tilePos.x)
            {
                if (movenow)
                {
                    movenow = false;
                    anim.Play("walks");
                    //anim.SetTrigger("moveside");
                    way = 1;
                    yield return new WaitForEndOfFrame();
                }

                if (!sp.flipX)
                {

                    sp.flipX = true;

                }

                this.transform.position = new Vector2(this.transform.position.x - speed, this.transform.position.y);

                if (this.transform.position.x < pos.x)
                {
                    this.transform.position = new Vector2(pos.x, this.transform.position.y);
                }

                Debug.Log("MOVE LEFT");
            }
            else if (player.tilePos.y < temp.tilePos.y)
            {
                if (movenow)
                {
                    movenow = false;
                    anim.Play("walk");
                    //anim.SetTrigger("move");
                    way = 2;
                    yield return new WaitForEndOfFrame();
                }

                if (sp.flipX)
                {
                    sp.flipX = false;
                }

                this.transform.position = new Vector2(this.transform.position.x, this.transform.position.y - speed);

                if (this.transform.position.y < pos.y)
                {
                    this.transform.position = new Vector2(this.transform.position.x, pos.y);
                }

                Debug.Log("MOVE DOWN");
            }
            else if (player.tilePos.y > temp.tilePos.y)
            {
                if (movenow)
                {
                    movenow = false;
                    anim.Play("walku");
                    //anim.SetTrigger("moveup");
                    way = 3;
                    yield return new WaitForEndOfFrame();
                }

                if (sp.flipX)
                {
                    sp.flipX = false;
                }

                this.transform.position = new Vector2(this.transform.position.x, this.transform.position.y + speed);

                if (this.transform.position.y > pos.y)
                {
                    this.transform.position = new Vector2(this.transform.position.x, pos.y);
                }

                Debug.Log("MOVE UP");
            }
            else
            {
                transform.position = pos;
            }

            if (pos.Equals(transform.position))
            {
                num--;

                if (num < 0)
                {
                    break;
                }

                temp = nodeList[num];
                playerPos = temp.tilePos;
                player = playerNode;

                if (player.tilePos.x < temp.tilePos.x)
                {
                    way2 = 0;
                }
                else if (player.tilePos.x > temp.tilePos.x)
                {
                    way2 = 1;
                }
                else if (player.tilePos.y < temp.tilePos.y)
                {
                    way2 = 2;
                }
                else if (player.tilePos.y > temp.tilePos.y)
                {
                    way2 = 3;
                }

                if (way != way2)
                {
                    movenow = true;
                }
            }

            yield return new WaitForEndOfFrame();

            limite++;
        } while (limite <= 150);

        if (limite != 0)
        {
            this.transform.position = new Vector2(pos.x, this.transform.position.y);
        }

        //Debug.Log("@@@@" + player.tilePos.x + "/" + player.tilePos.y);
        //Debug.Log("@@@@" + temp.tilePos.x + "/" + temp.tilePos.y);

        anim.SetFloat("speed", 0.0f);

        LookAt();

        stopcor = false;
        csSettings.instance.StopWalk();

        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "CanMove")
        {
            //Debug.Log("---" + col.gameObject.GetComponent<csTile>().tilePos.x + " / " + col.gameObject.GetComponent<csTile>().tilePos.y);
            //if (endPos != beforendPos )
            //{
            //    playerNode = col.GetComponent<csTile>();
            //    Debug.Log("----!" + playerNode.tilePos.x + " / " + playerNode.tilePos.y);
            //}
            playerNode = col.GetComponent<csTile>();
        }
    }

    public void OnNode(csTile tile, bool tree)
    {
        if (stopcor == true)
        {
            return;
        }
        csLevelManager.instance.BtnOff();

        csSettings.instance.PlayWalk();

        StopCoroutine(PlayerMove());

        this.transform.position = new Vector3(-15 + (playerNode.tilePos.x * 0.9f), 48.3f - (playerNode.tilePos.y * 0.9f) + 0.5f, 0);

        for (int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].nodecheck = false;
        }

        playerMove = false;
        openList.Clear();
        closeList.Clear();
        nodeList.Clear();

        treebool = tree;
        lastPos = endPos;
        endPos = tile;
        playerMoveCenter = true;

        if (lastPos != null)
        {
            if (lastPos.havetree)
            {
                lastPos.tree.isActiveFalse();
            }
        }

        if (endPos == null)
        {
            endPos = playerNode;
        }
        //Debug.Log(endPos.tilePos.x + " / " + endPos.tilePos.y);
        //Debug.Log(lastPos.tilePos.x + " // " + lastPos.tilePos.y);
    }

    //바라보는 방향
    void LookAt()
    {
        // Debug.Log("1");

        if (!treebool)
        {
            //Debug.Log("2");
            return;
        }
        else
        {
            //Debug.Log("3");
            treebool = false;
            canAx = true;
            csLevelManager.instance.BtnOn();
        }

        Point look = playerNode.tilePos;

        for (int i = 0; i < direction.Length; i++)
        {
            //Debug.Log("4");
            Point pos = new Point(look.x + direction[i].pos.x, look.y + direction[i].pos.y);

            if (pos.x < 0 || pos.x >= 50 || pos.y < 0 || pos.y >= 87)
            {
                //Debug.Log("5");
                continue;
            }

            csTile temp = tiles[pos.y, pos.x];

            if (!temp.havetree)
            {
                //Debug.Log("6");
                continue;
            }

            if (!temp.tree.active)
            {
                //Debug.Log("7");
                continue;
            }
            //Debug.Log("8");
            if (look.x == temp.tilePos.x)
            {
                if (look.y < temp.tilePos.y)
                {
                    //Debug.Log("d");
                    anim.Play("walk");
                    looktree = 3;
                }
                else if (look.y > temp.tilePos.y)
                {
                    //Debug.Log("u");
                    anim.Play("walku");
                    looktree = 2;
                }
            }
            else if (look.y == temp.tilePos.y)
            {
                if (look.x < temp.tilePos.x)
                {
                    //Debug.Log("r");
                    anim.Play("walks");
                    looktree = 0;
                    if (sp.flipX)
                    {
                        sp.flipX = false;
                    }
                }
                else if (look.x > temp.tilePos.x)
                {
                    //Debug.Log("l");
                    anim.Play("walks");
                    looktree = 1;
                    if (!sp.flipX)
                    {
                        sp.flipX = true;
                    }
                }
            }
        }
    }
}
