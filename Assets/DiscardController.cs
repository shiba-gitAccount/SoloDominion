using UnityEngine;
using System.Collections;

public class DiscardController : MonoBehaviour
{
    bool open = false;
    private Coroutine currentMoveCoroutine;
    public void OpenClose()
    {
        Debug.Log("Clicked!");
        if (currentMoveCoroutine != null) return;
        currentMoveCoroutine = StartCoroutine(MoveArea());
    }

    private IEnumerator MoveArea()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        float targetX = open ? -375f : 375f;
        Vector3 targetPos = new Vector3(startPos.x + targetX, startPos.y, startPos.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
        open = !open;
        currentMoveCoroutine = null;
    }
}
