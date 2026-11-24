using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di UI Image (background joystick). Handle = child Image.
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform background;
        [SerializeField] RectTransform handle;
        [SerializeField] float radius = 80f;
        [SerializeField] float deadZone = 0.1f;

        public Vector2 Direction { get; private set; }   // -1..1
        public bool IsHeld { get; private set; }
        public float HoldTime { get; private set; }

        public System.Action<Vector2, float> OnReleased; // arah, lama ditahan

        void Awake()
        {
            if (background == null) background = GetComponent<RectTransform>();
            if (handle == null && transform.childCount > 0) handle = transform.GetChild(0).GetComponent<RectTransform>();
        }

        void Update() { if (IsHeld) HoldTime += Time.deltaTime; }

        public void OnPointerDown(PointerEventData e) { IsHeld = true; HoldTime = 0; OnDrag(e); }

        public void OnDrag(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, e.position, e.pressEventCamera, out Vector2 local);
            Vector2 v = Vector2.ClampMagnitude(local / radius, 1f);
            Direction = v.magnitude < deadZone ? Vector2.zero : v;
            if (handle != null) handle.anchoredPosition = v * radius;
        }

        public void OnPointerUp(PointerEventData e)
        {
            OnReleased?.Invoke(Direction, HoldTime);
            IsHeld = false;
            Direction = Vector2.zero;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
        }
    }
}
