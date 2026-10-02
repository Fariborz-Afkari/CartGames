using CardGames.DeadManDraws.Core.Game;
using CardGames.DeadManDraws.Core.Players;

namespace CardGames.DeadManDraws.Core.AI
{
    public interface IAIStrategy
    {
        GameAction DecideAction(
            GameState state,
            PlayerState actor);
    }
}
