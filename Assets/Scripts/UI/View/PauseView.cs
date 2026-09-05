using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PauseView : UIView
    {
        [SerializeField] private Button resume;
        [SerializeField] private Button quit;
        
        public event Action Resume;
        public event Action Quit;

        private void OnEnable()
        {
            resume.onClick.AddListener(() => Resume?.Invoke());
            quit.onClick.AddListener(() => Quit?.Invoke());
        }

        private void OnDisable()
        {
            resume.onClick.RemoveAllListeners();
            quit.onClick.RemoveAllListeners();
        }
    }
}