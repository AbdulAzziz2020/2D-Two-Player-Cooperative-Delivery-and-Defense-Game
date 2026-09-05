using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class OnScreenView : UIView
    {
        [SerializeField] private Button pause;

        public event Action Pause;

        private void OnEnable()
        {
            pause.onClick.AddListener(() => Pause?.Invoke());
        }

        private void OnDisable()
        {
            pause.onClick.RemoveAllListeners();
        }
    }
}