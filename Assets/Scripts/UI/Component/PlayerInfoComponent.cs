using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class PlayerInfoComponent : UIComponent
    {
        [SerializeField] private TMP_Text playerName;
        [SerializeField] private TMP_Text clientId;

        public void SetPlayerInfo(string playerName, string clientId)
        {
            this.playerName.text = playerName;
            this.clientId.text = clientId;
        }
    }
}