using System.Collections.Generic;

namespace AngelVSZombie
{
    public class PlayerData
    {
        public PlayerDefinition Definition;
        public List<CardDefinition> Hand = new List<CardDefinition>();
        public bool IsAlive = true;
        public bool HasAngel = false; // آیا کارت فرشته را در دست دارد یا خیر

        public PlayerData(PlayerDefinition def)
        {
            Definition = def;
        }
    }
}