using UnityEngine;
using UnityEngine.EventSystems;

public class MeshTrigger : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        FindFirstObjectByType<EditorUIController>().MeshTriggered(gameObject.transform.localPosition);
    }
}
