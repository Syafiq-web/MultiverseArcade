using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// Komponen visual untuk satu Kartu Game pada menu Game Selection.
    /// Sesuai GDD Bagian 2.5.
    /// </summary>
    public class GameCardView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _genreText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private GameObject _comingSoonBadge;
        [SerializeField] private Button _playButton;
        [SerializeField] private TextMeshProUGUI _playButtonText;

        private GameInfo _boundInfo;
        private Action<GameInfo> _onPlayCallback;

        public void BindElements(Image thumbnail, TextMeshProUGUI title, TextMeshProUGUI genre,
            TextMeshProUGUI description, GameObject comingSoonBadge, Button playButton, TextMeshProUGUI playButtonText)
        {
            _thumbnailImage = thumbnail;
            _titleText = title;
            _genreText = genre;
            _descriptionText = description;
            _comingSoonBadge = comingSoonBadge;
            _playButton = playButton;
            _playButtonText = playButtonText;
        }

        public void Setup(GameInfo info, Action<GameInfo> onPlayCallback)
        {
            _boundInfo = info;
            _onPlayCallback = onPlayCallback;

            if (_titleText != null) _titleText.text = info.displayName;
            if (_genreText != null) _genreText.text = info.genre;
            if (_descriptionText != null) _descriptionText.text = info.description;

            if (_thumbnailImage != null && info.thumbnail != null)
            {
                _thumbnailImage.sprite = info.thumbnail;
                _thumbnailImage.color = Color.white;
            }

            if (_comingSoonBadge != null)
            {
                _comingSoonBadge.SetActive(!info.isEnabled);
            }

            if (_playButton != null)
            {
                _playButton.interactable = info.isEnabled;
                _playButton.onClick.RemoveAllListeners();

                if (info.isEnabled)
                {
                    _playButton.onClick.AddListener(OnPlayClicked);
                    if (_playButtonText != null) _playButtonText.text = "PLAY";
                }
                else
                {
                    if (_playButtonText != null) _playButtonText.text = "LOCKED";
                }
            }
        }

        private void OnPlayClicked()
        {
            _onPlayCallback?.Invoke(_boundInfo);
        }
    }
}
