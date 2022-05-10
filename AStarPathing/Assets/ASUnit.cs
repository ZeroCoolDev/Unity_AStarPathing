using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASUnit : MonoBehaviour
{
    public Transform target;
    float speed = 20f;
    Vector3[] path;
    int targetIndex;

    void Start()
    {
        ASRequestPathManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool bPathSuccess)
    {
        if(bPathSuccess)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];
        while(true)
        {
            if(transform.position == currentWaypoint)
            {
                ++targetIndex;
                if(targetIndex >= path.Length)
                {// we have reached the end of our path!
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }
            // Move closer to the waypoint
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if(path != null)
        {
            for(int i = targetIndex; i < path.Length; ++i)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if(i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else{
                    Gizmos.DrawLine(path[i-1], path[i]);
                }
            }
        }
    }
}
