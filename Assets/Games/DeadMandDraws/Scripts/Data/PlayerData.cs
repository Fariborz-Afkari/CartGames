using System.Collections.Generic;

namespace DeadManDraws
{
    public class PlayerData
    {
        public int PlayerId;
        public string PlayerName;
        public int AvatarIndex;
        public bool IsHuman;
        public TraitType Trait;
        public int Hardness; // for AI

        public List<CardDefinition> Bank = new List<CardDefinition>();
        public List<CardDefinition> Board = new List<CardDefinition>();

        public int SafeDrawsRemaining;
        public int DavyJonesOwnerId = -1; // who targets this player

        public int BankCardCount => Bank.Count;

        public int ComputeScore()
        {
            var best = new Dictionary<CardType, int>();
            foreach (var c in Bank)
            {
                int v = c.Value;
                if (c.CardType == CardType.Mermaid && Trait == TraitType.GoldenScales) v += 5;
                if (!best.TryGetValue(c.CardType, out var cur) || v > cur) best[c.CardType] = v;
            }
            int total = 0;
            foreach (var v in best.Values) total += v;
            return total;
        }

        public bool HasCardInBank(CardType t)
        {
            foreach (var c in Bank) if (c.CardType == t) return true;
            return false;
        }
    }
}