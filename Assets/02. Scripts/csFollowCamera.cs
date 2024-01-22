using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//플레이어를 따라다니는 카메라
public class csFollowCamera : MonoBehaviour
{
    public Transform player;

    public Vector2 maxXandY;
    public Vector2 minXandY;

    public float xMargin = 1f;
    public float yMargin = 1f;

    public float xSmooth = 6f;
    public float ySmooth = 6f;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    bool CheckXMargin()
    {
        return Mathf.Abs(transform.position.x - player.position.x) > xMargin;
    }

    bool CheckYMargin()
    {
        return Mathf.Abs(transform.position.y - player.position.y) > yMargin;
    }

    private void LateUpdate()
    {
        TrackPlayer();
    }

    //카메라가 플레이어를 따라다님
    void TrackPlayer()
    {
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        if (CheckXMargin())
        {
            targetX = Mathf.Lerp(transform.position.x, player.position.x, xSmooth * Time.deltaTime);
        }

        if (CheckYMargin())
        {
            targetY = Mathf.Lerp(transform.position.y, player.position.y, ySmooth * Time.deltaTime);
        }

        targetX = Mathf.Clamp(targetX, minXandY.x, maxXandY.x);
        targetY = Mathf.Clamp(targetY, minXandY.y, maxXandY.y);

        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }
}
