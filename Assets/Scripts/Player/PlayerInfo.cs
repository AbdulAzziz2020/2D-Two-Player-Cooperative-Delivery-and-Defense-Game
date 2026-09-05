using System;

namespace Game.UI
{
    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string playerName;

        public PlayerInfo(string playerId, string playerName)
        {
            this.playerId = playerId;
            this.playerName = playerName;
        }
    }
    
    
}