using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MessageView : UIView
    {
        [SerializeField] private TMP_Text message;
        [SerializeField] private Button disconnect;
        
        public event Action Disconnect;

        private void OnEnable()
        {
            disconnect.onClick.AddListener(() => Disconnect?.Invoke());
        }
        
        private void OnDisable()
        {
            disconnect.onClick.RemoveAllListeners();
        }

        public void SetMessage(string message)
        {
            this.message.text = message;
        }
        
        public void SetButtonText(string text)
        {
            disconnect.GetComponentInChildren<TMP_Text>().text = text;
        }
    }
}