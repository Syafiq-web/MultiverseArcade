using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di Button gadget. Menampilkan sisa pakai dan meredup saat habis.
    [RequireComponent(typeof(Button))]
    public class GadgetButtonUI : MonoBehaviour
    {
        [SerializeField] GadgetSlot slot;
        [SerializeField] Text usesText;          // ganti ke TMP_Text jika project memakai TextMeshPro
        [SerializeField] CanvasGroup group;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (slot != null) slot.Use();
            });
        }

        void OnEnable()
        {
            if (slot != null)
            {
                slot.OnUsesChanged += Refresh;
                Refresh(slot.UsesLeft);
            }
        }

        void OnDisable()
        {
            if (slot != null) slot.OnUsesChanged -= Refresh;
        }

        public void Setup(GadgetSlot gadgetSlot, Text text = null, CanvasGroup canvasGroup = null)
        {
            if (slot != null) slot.OnUsesChanged -= Refresh;
            slot = gadgetSlot;
            usesText = text;
            group = canvasGroup;
            if (slot != null && enabled)
            {
                slot.OnUsesChanged += Refresh;
                Refresh(slot.UsesLeft);
            }
        }

        void Refresh(int uses)
        {
            if (usesText != null) usesText.text = $"{uses}";
            if (group != null) group.alpha = uses > 0 ? 1f : 0.35f;
            GetComponent<Button>().interactable = uses > 0;
        }
    }
}
