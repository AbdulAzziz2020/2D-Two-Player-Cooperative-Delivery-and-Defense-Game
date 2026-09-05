using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class SessionView : UIView
    {
        [SerializeField] private TMP_InputField inputName;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;

        public event Action Host;
        public event Action Client;
        public event Action<string> NameChanged;

        private void OnEnable()
        {
            hostButton.onClick.AddListener(HandleHost);
            clientButton.onClick.AddListener(HandleClient);

            inputName.onValueChanged.AddListener(HandleNameChanged);
        }

        private void OnDisable()
        {
            hostButton.onClick.RemoveListener(HandleHost);
            clientButton.onClick.RemoveListener(HandleClient);

            inputName.onValueChanged.RemoveListener(HandleNameChanged);
        }

        protected override void OnShow()
        {
            inputName.text = "";
            SetButtons(true);
        }

        public void SetButtons(bool enabled)
        {
            hostButton.interactable = enabled;
            clientButton.interactable = enabled;
        }

        private void HandleHost()
        {
            Host?.Invoke();
        }

        private void HandleClient()
        {
            Client?.Invoke();
        }

        private void HandleNameChanged(string value)
        {
            NameChanged?.Invoke(value);
        }
    }
}