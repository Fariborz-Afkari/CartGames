using System.Collections.Generic;
using CardGames.DeadManDraws.Core.Cards;
using CardGames.DeadManDraws.Core.Players;

namespace CardGames.DeadManDraws.Core.Game
{
    public static class GameRules
    {
        public static bool IsValidDraw(
            GameState state,
            PlayerState player)
        {
            if (state == null ||
                player == null)
                return false;

            if (state.IsGameOver)
                return false;

            if (state.CurrentPlayerId !=
                player.Id)
            {
                return false;
            }

            if (player.HasDrawnAtLeastOne &&
                state.Deck.IsEmpty)
            {
                return false;
            }

            return !state.Deck.IsEmpty;
        }

        public static bool IsBust(
            PlayerState player,
            Card card)
        {
            if (player == null ||
                card == null)
            {
                return false;
            }

            return player.PlayArea
                .ContainsType(card.Type);
        }

        public static void MoveToBank(
            PlayerState player)
        {
            if (player == null)
                return;

            for (int i = 0;
                 i < player.PlayArea.Count;
                 i++)
            {
                player.Bank.Add(
                    player.PlayArea.Cards[i]);
            }

            player.PlayArea.Clear();
        }

        public static void DiscardPlayArea(
            GameState state,
            PlayerState player)
        {
            if (state == null ||
                player == null)
                return;

            for (int i = 0;
                 i < player.PlayArea.Count;
                 i++)
            {
                state.AddToDiscard(
                    player.PlayArea.Cards[i]);
            }

            player.PlayArea.Clear();
        }

        public static int CalculateScore(
            PlayerState player)
        {
            if (player == null)
                return 0;

            Dictionary<CardType, int>
                highest =
                    new Dictionary<CardType, int>();

            for (int i = 0;
                 i < player.Bank.Count;
                 i++)
            {
                Card card =
                    player.Bank.Cards[i];

                int value =
                    card.Value;

                if (card.Type ==
                    CardType.Mermaid &&
                    player.HasTrait(
                        PlayerTrait.GoldenScales))
                {
                    value += 5;
                }

                int current;

                if (!highest.TryGetValue(
                        card.Type,
                        out current))
                {
                    highest.Add(
                        card.Type,
                        value);
                }
                else if (value > current)
                {
                    highest[card.Type] =
                        value;
                }
            }

            int score = 0;

            foreach (int value
                     in highest.Values)
            {
                score += value;
            }

            return score;
        }

        public static PlayerState
            FindWinner(
                IReadOnlyList<PlayerState>
                    players)
        {
            PlayerState winner = null;

            int bestScore = -1;
            int bestCardCount = -1;

            for (int i = 0;
                 i < players.Count;
                 i++)
            {
                PlayerState player =
                    players[i];

                int score =
                    CalculateScore(player);

                int cardCount =
                    player.Bank.Count;

                if (winner == null ||
                    score > bestScore ||
                    (score == bestScore &&
                     cardCount > bestCardCount))
                {
                    winner = player;
                    bestScore = score;
                    bestCardCount = cardCount;
                }
            }

            return winner;
        }
    }
}