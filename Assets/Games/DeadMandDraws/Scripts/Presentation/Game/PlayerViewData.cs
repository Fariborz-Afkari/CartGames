using CardGames.DeadManDraws.Core.Players;

namespace CardGames.DeadManDraws.Presentation.Game
{
    public sealed class PlayerViewData
    {
        public int PlayerId { get; }

        public string PlayerName { get; }

        public bool IsHuman { get; }

        public int Score { get; }

        public int BankCardCount { get; }

        public int PlayAreaCardCount { get; }

        public PlayerTrait Trait { get; }

        public int AvatarIndex { get; }

        public PlayerViewData(
            int playerId,
            string playerName,
            bool isHuman,
            int score,
            int bankCardCount,
            int playAreaCardCount,
            PlayerTrait trait,
            int avatarIndex)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            IsHuman = isHuman;
            Score = score;
            BankCardCount = bankCardCount;
            PlayAreaCardCount = playAreaCardCount;
            Trait = trait;
            AvatarIndex = avatarIndex;
        }
    }
}