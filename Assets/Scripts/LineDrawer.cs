using System.Collections;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    public bool isPaused = false;
    public IEnumerator DrawFromTo(SirVector start, SirVector end, float duration)
    {
        line.SetPosition(0, start.GetVector());
        float time = 0f;
        while (time < duration)
        {
            if (!isPaused)
            {
                line.SetPosition(1, Vector2.Lerp(start.GetVector(), end.GetVector(), time / duration));
                time += Time.deltaTime;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
