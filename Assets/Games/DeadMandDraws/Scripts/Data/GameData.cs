using System.Collections.Generic;

namespace DeadManDraws
{
    public class GameData
    {
        public List<PlayerData> Players = new List<PlayerData>();
        public List<CardDefinition> DrawDeck = new List<CardDefinition>();
        public List<CardDefinition> BurnDeck = new List<CardDefinition>();
        public List<CardDefinition> AllPool = new List<CardDefinition>();

        public int CurrentPlayerIndex;
        public int PlayerCount;
        public int Hardness;
        public bool GameOver;
    }
}