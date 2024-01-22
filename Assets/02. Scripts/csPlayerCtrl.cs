using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//플레이어 방향 설정
[System.Serializable]
public struct Direction
{
    public Point _pos;
    public float _g;

    public Direction(Point pos, float g)
    {
        _pos = pos;
        _g = g;
    }

    public Direction(int x, int y, float g)
    {
        _pos.x = x;
        _pos.y = y;
        _g = g;
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

    //A*알고리즘
    private List<csTile> openList;
    private List<csTile> closeList;
    private List<csTile> nodeList;

    public List<csTile> NodeList
    {
        get { return nodeList; }
        set { nodeList = value; }
    }

    //전후 좌후
    private Direction[] direction = {
        new Direction(1, 0, 10f), new Direction(-1, 0, 10f),
        new Direction(0, 1, 10f), new Direction(0, -1, 10f)
    };

    private Animator anim;

    private SpriteRenderer spriteRenderer;

    private bool treebool = false;

    private GameObject targetObj = null;
    private csSetTarget csSetTarget;

    private bool stopcor = false;

    [HideInInspector]
    public bool canAx = false;
    private bool onbtn = false;

    private int lookTree = 0;
    private bool isPlayHitAnim = false;

    private void Awake()
    {
        instance = this;

        tiles = new csTile[87, 50];

        openList = new List<csTile>();
        closeList = new List<csTile>();
        nodeList = new List<csTile>();

        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        playerNode = tiles[50, 19];
        playerPos = playerNode.tilePos;
        beforendPos = playerNode;
        transform.position = new Vector3(-15 + (playerPos.x * 0.9f), 48.3f - (playerPos.y * 0.9f) + 0.5f, 0);

        targetObj = Instantiate(csInitData.instance.target, transform);
        csSetTarget = targetObj.GetComponent<csSetTarget>();
    }

    private void Update()
    {
        //유니티 에디터 제어
#if UNITY_EDITOR
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
            csSetTarget.check = false;

        }
        //핸드폰 제어
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

            if (SetNode())
            {
                FindNode();
                StartCoroutine(PlayerMove());
            }
        }

        if (onbtn && canAx)
        {
            DoAttack();
        }
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

    //공격
    private void DoAttack()
    {
        StartCoroutine(AttackSound());

        isPlayHitAnim = true;
        StartCoroutine(AttackPlayAnim());

        if (isPlayHitAnim)
        {
            if (lookTree == 0 || lookTree == 1)
            {
                anim.Play("attacks");
            }
            else if (lookTree == 2)
            {
                anim.Play("attacku");
            }
            else if (lookTree == 3)
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
        csSettings.instance.PlayEffect(transform.position, Random.Range(5, 8), false);

        yield return null;
    }

    IEnumerator AttackPlayAnim()
    {
        yield return new WaitForSeconds(0.8f);
        isPlayHitAnim = false;

        yield return null;
    }

    public void OnButtonCheck()
    {
        if (isPlayHitAnim == false)
        {
            onbtn = true;
        }
    }

    //A*알고리즘
    bool SetNode()
    {
        startPos = playerNode;

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
            Point pos = new Point(tile.tilePos.x + direction[i]._pos.x, tile.tilePos.y + direction[i]._pos.y);

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
                temp.g = tile.g + direction[i]._g;
                temp.h = Mathf.Abs(endPos.tilePos.x - pos.x) + Mathf.Abs(endPos.tilePos.y - pos.y);
                temp.f = temp.g + temp.h;
                temp.parentTile = tile;
                tiles[pos.y, pos.x] = temp;
                openList.Add(temp);
            }
            else if (tiles[pos.y, pos.x].g > tile.g + 10)
            {
                temp.g = tile.g + direction[i]._g;
                temp.f = temp.g + temp.h;
                temp.parentTile = tile;
            }
        }
    }

    void FindNode()
    {
        csTile temp;

        if (treebool)
        {
            temp = tiles[endPos.tilePos.y, endPos.tilePos.x].parentTile;
        }
        else
        {
            temp = tiles[endPos.tilePos.y, endPos.tilePos.x];
        }

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

        for (int i = 0; i < nodeList.Count; i++)
        {
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
            }
        }
        else if (player.tilePos.x > temp.tilePos.x)
        {
            if (movenow)
            {
                movenow = false;
                anim.Play("walks");
            }
        }
        else if (player.tilePos.y < temp.tilePos.y)
        {
            if (movenow)
            {
                movenow = false;
                anim.Play("walk");
            }
        }
        else if (player.tilePos.y > temp.tilePos.y)
        {
            if (movenow)
            {
                movenow = false;
                anim.Play("walku");
            }
        }

        yield return new WaitForEndOfFrame();

        int limit = 0;
        Vector3 pos = new Vector3(-15 + (temp.tilePos.x * 0.9f), 48.3f - (temp.tilePos.y * 0.9f) + 0.5f, 0);

        do
        {
            if (!playerMove)
            {
                break;
            }

            pos = new Vector3(-15 + (temp.tilePos.x * 0.9f), 48.3f - (temp.tilePos.y * 0.9f) + 0.5f, 0);

            if (player.tilePos.x < temp.tilePos.x)
            {
                limit = 0;

                if (movenow)
                {
                    movenow = false;
                    anim.Play("walks");
                    way = 0;
                    yield return new WaitForEndOfFrame();
                }

                if (spriteRenderer.flipX)
                {
                    spriteRenderer.flipX = false;
                }

                transform.position = new Vector2(transform.position.x + speed, transform.position.y);

                if (transform.position.x > pos.x)
                {
                    transform.position = new Vector2(pos.x, transform.position.y);
                }
                Debug.Log("MOVE RIGHT");

            }
            else if (player.tilePos.x > temp.tilePos.x)
            {
                if (movenow)
                {
                    movenow = false;
                    anim.Play("walks");
                    way = 1;
                    yield return new WaitForEndOfFrame();
                }

                if (!spriteRenderer.flipX)
                {

                    spriteRenderer.flipX = true;

                }

                transform.position = new Vector2(transform.position.x - speed, transform.position.y);

                if (transform.position.x < pos.x)
                {
                    transform.position = new Vector2(pos.x, transform.position.y);
                }

                Debug.Log("MOVE LEFT");
            }
            else if (player.tilePos.y < temp.tilePos.y)
            {
                if (movenow)
                {
                    movenow = false;
                    anim.Play("walk");
                    way = 2;
                    yield return new WaitForEndOfFrame();
                }

                if (spriteRenderer.flipX)
                {
                    spriteRenderer.flipX = false;
                }

                transform.position = new Vector2(transform.position.x, transform.position.y - speed);

                if (transform.position.y < pos.y)
                {
                    transform.position = new Vector2(transform.position.x, pos.y);
                }

                Debug.Log("MOVE DOWN");
            }
            else if (player.tilePos.y > temp.tilePos.y)
            {
                if (movenow)
                {
                    movenow = false;
                    anim.Play("walku");
                    way = 3;
                    yield return new WaitForEndOfFrame();
                }

                if (spriteRenderer.flipX)
                {
                    spriteRenderer.flipX = false;
                }

                transform.position = new Vector2(transform.position.x, transform.position.y + speed);

                if (transform.position.y > pos.y)
                {
                    transform.position = new Vector2(transform.position.x, pos.y);
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

            limit++;
        } while (limit <= 150);

        if (limit != 0)
        {
            transform.position = new Vector2(pos.x, transform.position.y);
        }

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

        transform.position = new Vector3(-15 + (playerNode.tilePos.x * 0.9f), 48.3f - (playerNode.tilePos.y * 0.9f) + 0.5f, 0);

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
    }

    //바라보는 방향
    void LookAt()
    {
        if (!treebool)
        {
            return;
        }
        else
        {
            treebool = false;
            canAx = true;
            csLevelManager.instance.BtnOn();
        }

        Point look = playerNode.tilePos;

        for (int i = 0; i < direction.Length; i++)
        {
            Point pos = new Point(look.x + direction[i]._pos.x, look.y + direction[i]._pos.y);

            if (pos.x < 0 || pos.x >= 50 || pos.y < 0 || pos.y >= 87)
            {
                continue;
            }

            csTile temp = tiles[pos.y, pos.x];

            if (!temp.havetree)
            {
                continue;
            }

            if (!temp.tree.active)
            {
                continue;
            }

            if (look.x == temp.tilePos.x)
            {
                if (look.y < temp.tilePos.y)
                {
                    anim.Play("walk");
                    lookTree = 3;
                }
                else if (look.y > temp.tilePos.y)
                {
                    anim.Play("walku");
                    lookTree = 2;
                }
            }
            else if (look.y == temp.tilePos.y)
            {
                if (look.x < temp.tilePos.x)
                {
                    anim.Play("walks");
                    lookTree = 0;
                    if (spriteRenderer.flipX)
                    {
                        spriteRenderer.flipX = false;
                    }
                }
                else if (look.x > temp.tilePos.x)
                {
                    anim.Play("walks");
                    lookTree = 1;
                    if (!spriteRenderer.flipX)
                    {
                        spriteRenderer.flipX = true;
                    }
                }
            }
        }
    }
}
