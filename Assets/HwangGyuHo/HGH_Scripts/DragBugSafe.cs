using UnityEngine;
using UnityEngine.EventSystems;

public class DragBugSafe : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 처리 로직
        if (eventData != null)
            return;
    }
}
