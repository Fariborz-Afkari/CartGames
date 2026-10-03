using CardGames.DeadManDraws.Core.Game;
using CardGames.DeadManDraws.Core.Players;

namespace CardGames.DeadManDraws.Core.AI
{
    public sealed class BasicAiStrategy : IAIStrategy
    {
        public GameAction DecideAction(
            GameState state,
            PlayerState actor)
        {
            if (state == null ||
                actor == null)
            {
                return null;
            }

            if (state.IsGameOver)
                return null;

            if (state.CurrentPlayerId != actor.Id)
                return null;

            /*
             * اگر قبلاً کارت گرفته‌ایم،
             * فعلاً تصمیم ساده AI این است که
             * منطقه را Bank کند.
             */
            if (actor.HasDrawnAtLeastOne &&
                actor.PlayArea.Count > 0)
            {
                return GameAction.BankPlayArea(
                    actor.Id);
            }

            /*
             * اولین حرکت:
             * Draw
             */
            return GameAction.DrawCard(
                actor.Id);
        }
    }
}