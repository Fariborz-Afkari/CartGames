using System.Collections.Generic;

namespace DeadManDraws
{
    public class AiPlayer
    {
        private readonly GameEngine _engine;
        private readonly System.Random _rng = new System.Random();

        public AiPlayer(GameEngine engine) { _engine = engine; }

        public bool WantsToDraw(PlayerData p)
        {
            int board = p.Board.Count;
            int max = 12;
            if (board >= max) return false;

            // heuristic: risk vs reward
            int riskyCount = 0;
            var typeCounts = new Dictionary<CardType, int>();
            foreach (var c in p.Board)
            {
                typeCounts.TryGetValue(c.CardType, out var n);
                typeCounts[c.CardType] = n + 1;
            }

            // difficulty scaling
            int threshold = 4 + p.Hardness; // 5..7
            if (board >= threshold) return false;

            // Randomize for lower difficulties
            if (p.Hardness == 1 && _rng.Next(100) < 40) return false;
            if (p.Hardness == 2 && _rng.Next(100) < 20) return false;

            return true;
        }

        public PlayerData PickCannonTarget(CardType type)
        {
            PlayerData best = null;
            int bestValue = -1;
            foreach (var other in _engine.Data.Players)
            {
                if (other == _engine.CurrentPlayer) continue;
                foreach (var c in other.Bank)
                {
                    if (c.CardType == type && c.Value > bestValue)
                    {
                        bestValue = c.Value;
                        best = other;
                    }
                }
            }
            return best ?? PickAnyOpponentWithCards();
        }

        public PlayerData PickSwordTarget()
        {
            PlayerData best = null;
            int bestValue = -1;
            foreach (var other in _engine.Data.Players)
            {
                if (other == _engine.CurrentPlayer) continue;
                foreach (var c in other.Bank)
                    if (c.Value > bestValue) { bestValue = c.Value; best = other; }
            }
            return best;
        }

        public CardDefinition PickHookCard()
        {
            var p = _engine.CurrentPlayer;
            if (p.Bank.Count == 0) return null;
            // Pick lowest value to minimize risk
            CardDefinition low = p.Bank[0];
            foreach (var c in p.Bank) if (c.Value < low.Value) low = c;
            return low;
        }

        public CardDefinition PickMapCard()
        {
            var list = _engine.PendingMapChoices;
            if (list == null || list.Count == 0) return null;
            CardDefinition best = list[0];
            foreach (var c in list) if (c.Value > best.Value) best = c;
            return best;
        }

        public bool WantsToContinueAfterOracle(PlayerData p)
        {
            return p.Board.Count < 3 + p.Hardness;
        }

        PlayerData PickAnyOpponentWithCards()
        {
            foreach (var other in _engine.Data.Players)
                if (other != _engine.CurrentPlayer && other.Bank.Count > 0) return other;
            return null;
        }
    }
}