using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class PlayerListView : UIView
    {
        [SerializeField] private PlayerInfoComponent[] playerInfos;
        [SerializeField] private TMP_Text stateText;

        public void SetPlayerList(PlayerSession[] players)
        {
            int count = Mathf.Min(players.Length, playerInfos.Length);

            for (int i = 0; i < playerInfos.Length; i++)
            {
                playerInfos[i].Hide();
            }

            for (int i = 0; i < count; i++)
            {
                PlayerSession player = players[i];
                
                playerInfos[i].SetPlayerInfo(player.name.ToString(), player.clientId.ToString());
                playerInfos[i].Show();
            }
        }

        public void SetStateText(string text)
        {
            stateText.text = text;
        }
    }
}