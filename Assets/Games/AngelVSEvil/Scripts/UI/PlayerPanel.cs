using System;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

namespace AngelVSZombie
{
    public class PlayerPanel : MonoBehaviour
    {
        [Header("UI Components")]
        public Image AvatarImage;
        public RTLTextMeshPro txtPlayerName;
        public Image BackgroundImage;
        public Button TargetButton;

        [Header("Turn Indicator (نشانگر نوبت)")]
        public GameObject TurnIndicator; // مثلاً یک Image (حاشیه زرد/طلایی) یا یک فلش

        [Header("Background Sprites")]
        public Sprite AliveBackground;
        public Sprite DeadBackground;

        private PlayerData _playerData;
        private Action<PlayerData> _onTargetClicked;

        private void Awake()
        {
            // پیدا کردن دکمه به صورت خودکار اگر در Inspector وصل نشده باشد
            if (TargetButton == null) TargetButton = GetComponent<Button>();
            if (TargetButton == null) TargetButton = GetComponentInChildren<Button>();

            if (TargetButton != null)
            {
                TargetButton.onClick.RemoveAllListeners(); // جلوگیری از ثبت چندباره
                TargetButton.onClick.AddListener(HandleClick);
            }
        }

        //public void Setup(PlayerData player, Sprite[] avatarSprites)
        //{
        //    _playerData = player;

        //    if (txtPlayerName != null) txtPlayerName.text = player.Definition.PlayerName;

        //    if (AvatarImage != null && avatarSprites != null && avatarSprites.Length > 0)
        //    {
        //        int idx = Mathf.Clamp(player.Definition.AvatarIndex, 0, avatarSprites.Length - 1);
        //        if (avatarSprites[idx] != null)
        //        {
        //            AvatarImage.sprite = avatarSprites[idx];
        //            AvatarImage.preserveAspect = true;
        //        }
        //    }

        //    // در حالت عادی غیرفعال است
        //    if (TargetButton != null) TargetButton.interactable = false;

        //    UpdateVisuals();
        //}
        public void Setup(PlayerData player, Sprite[] avatarSprites)
        {
            _playerData = player;
            if (txtPlayerName != null) txtPlayerName.text = player.Definition.PlayerName;
            if (AvatarImage != null && avatarSprites != null && avatarSprites.Length > 0)
            {
                int idx = Mathf.Clamp(player.Definition.AvatarIndex, 0, avatarSprites.Length - 1);
                if (avatarSprites[idx] != null)
                {
                    AvatarImage.sprite = avatarSprites[idx];
                    AvatarImage.preserveAspect = true;
                }
            }
            if (TargetButton != null) TargetButton.interactable = false;

            SetTurn(false); // در ابتدا خاموش است
            UpdateVisuals();
        }
        public void SetAsTarget(Action<PlayerData> onTargetClicked)
        {
            _onTargetClicked = onTargetClicked;
            // دکمه را برای کلیک فعال می‌کنیم
            if (TargetButton != null) TargetButton.interactable = true;
        }

        private void HandleClick()
        {
            Debug.Log($"[PlayerPanel] کلیک شد روی: {_playerData?.Definition.PlayerName}");
            _onTargetClicked?.Invoke(_playerData);
        }

        public void UpdateVisuals()
        {
            if (_playerData == null) return;

            if (BackgroundImage != null)
            {
                Sprite target = _playerData.IsAlive ? AliveBackground : DeadBackground;
                if (target != null) BackgroundImage.sprite = target;
            }

            if (AvatarImage != null)
            {
                AvatarImage.color = _playerData.IsAlive ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }

        public void SetTurn(bool isActive)
        {
            if (TurnIndicator != null)
            {
                TurnIndicator.SetActive(isActive);
            }
        }
    }
}