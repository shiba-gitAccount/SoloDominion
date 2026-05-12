using UnityEngine;
using System.Collections;

public class CardMovement : MonoBehaviour
{
    public void MoveTo(Transform destination, float duration, bool destroyOnArrival)
    {
        StartCoroutine(MoveRoutine(destination, duration, destroyOnArrival));
    }

    private IEnumerator MoveRoutine(Transform dest, float time, bool destroyOnArrival)
    {
        Vector3 startPos = transform.position;
        Vector3 goal = dest.position;
        float elapsed = 0;

        while (elapsed < time )
        {
            float t = elapsed / time;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, goal, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = goal;
        if (destroyOnArrival) Destroy(gameObject);
        else transform.SetParent(dest);
    }
}
