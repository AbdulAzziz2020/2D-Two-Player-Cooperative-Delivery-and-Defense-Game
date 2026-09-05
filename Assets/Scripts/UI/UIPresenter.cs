using UnityEngine;

namespace Game.UI
{
    public abstract class UIPresenter<T> : MonoBehaviour where T : UIView
    {
        [SerializeField] protected T view;
    }
}