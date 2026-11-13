using UnityEngine;
using UnityEngine.UI;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// Controller UI untuk panel Kredit (Daftar tim pengembang & Lisensi aset).
    /// </summary>
    public class CreditsUI : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        public void BindCloseButton(Button btn)
        {
            _closeButton = btn;
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void OnCloseClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
            gameObject.SetActive(false);
        }
    }
}
