using UnityEngine;

public class HandController : MonoBehaviour
{
    float pi = 3.14159265359f;
    private float radius = 2000f;

    void OnTransformChildrenChanged()
    {
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;
        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            float index_f = (1f / (childCount + 1) * (i + 1) - 0.5f) / 6f;
            child.anchorMin = new Vector2(0.5f, 0.5f);
            child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.anchoredPosition = new Vector2(radius * ApproSin(index_f * pi), radius * ApproCos(index_f * pi) - radius);
            child.rotation = Quaternion.Euler(0, 0, - index_f * 180f);
        }
    }

    static float ApproSin(float radians)
    {
        float radians3 = radians * radians * radians;
        return radians - (radians3 / 6f);
    }

    static float ApproCos(float radians)
    {
        float radians2 = radians * radians;
        return 1f - (radians2 / 2f);
    }
}
