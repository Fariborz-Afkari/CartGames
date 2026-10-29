namespace AngelVSZombie
{
    public class GameData
    {
        public System.Collections.Generic.List<CardDefinition> Deck = new System.Collections.Generic.List<CardDefinition>();
        public System.Collections.Generic.List<CardDefinition> DiscardPile = new System.Collections.Generic.List<CardDefinition>();
        public System.Collections.Generic.List<PlayerData> Players = new System.Collections.Generic.List<PlayerData>();

        // این لیست جدید را اضافه کنید
        public System.Collections.Generic.List<CardDefinition> Graveyard = new System.Collections.Generic.List<CardDefinition>();

        public int CurrentPlayerIndex = 0;
        public int ExtraTurns = 0;
        public bool SkipDraw = false;
        public CardDefinition LastPlayedCard = null;
        public PlayerData LastPlayedPlayer = null;
    }
}