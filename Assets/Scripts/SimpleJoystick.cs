using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;
    private Vector2 inputVector = Vector2.zero;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Canvas canvas = background.GetComponentInParent<Canvas>().rootCanvas;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, cam, out pos))
        {
            float bgWidth = background.rect.width;
            pos.x = Mathf.Clamp(pos.x / (bgWidth / 2f), -1f, 1f);
            inputVector = new Vector2(pos.x, 0f);
            handle.anchoredPosition = new Vector2(inputVector.x * (bgWidth / 2f - handle.rect.width / 2f), 0f);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetJoystick();
    }

    void OnDisable()
    {
        ResetJoystick();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) ResetJoystick();
    }

    void ResetJoystick()
    {
        inputVector = Vector2.zero;
        if (handle != null) handle.anchoredPosition = Vector2.zero;
    }

    public float Horizontal()
    {
        return inputVector.x;
    }
}