using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ResultView : UIView
    {
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button disconnect;
        [SerializeField] private Button retry;

        public event Action Disconnect;
        public event Action Retry;
        
        private void OnEnable()
        {
            disconnect.onClick.AddListener(() => Disconnect?.Invoke());
            retry.onClick.AddListener(() => Retry?.Invoke());
        }
        
        private void OnDisable()
        {
            disconnect.onClick.RemoveAllListeners();
            retry.onClick.RemoveAllListeners();
        }

        public ResultView SetResultText(string text)
        {
            resultText.text = text;
            return this;
        }

        public ResultView SetButtonDisconnect(bool enabled)
        {
            disconnect.gameObject.SetActive(enabled);
            disconnect.interactable = enabled;
            return this;
        }

        public ResultView SetButtonRetry(bool enabled)
        {
            retry.gameObject.SetActive(enabled);
            retry.interactable = enabled;
            return this;
        }
    }
}