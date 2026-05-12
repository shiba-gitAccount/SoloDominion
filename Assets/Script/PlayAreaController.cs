using UnityEngine;

public class PlayAreaController : MonoBehaviour
{
    private float width = 1000f;
    void OnTransformChildrenChanged()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;
        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            float index_f = (1f / (childCount + 1) * (i + 1) - 0.5f);
            child.anchoredPosition = new Vector2(width * index_f, 0);
            child.rotation = Quaternion.identity;
        }
    }
}
