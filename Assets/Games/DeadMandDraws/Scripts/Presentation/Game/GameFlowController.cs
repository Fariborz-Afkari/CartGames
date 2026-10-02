using CardGames.DeadManDraws.Core.AI;
using CardGames.DeadManDraws.Core.Game;
using CardGames.DeadManDraws.Core.Players;

namespace CardGames.DeadManDraws.Presentation.Game
{
    public sealed class GameFlowController
    {
        private readonly PirateGameEngine _engine;
        private readonly IAIStrategy _aiStrategy;

        public GameFlowController(
            PirateGameEngine engine,
            IAIStrategy aiStrategy)
        {
            _engine = engine;
            _aiStrategy = aiStrategy;
        }

        public void RunAiTurns()
        {
            while (!_engine.State.IsGameOver)
            {
                PlayerState current =
                    _engine.State.FindPlayer(
                        _engine.State.CurrentPlayerId);

                if (current == null ||
                    current.IsHuman)
                {
                    return;
                }

                if (!RunAiTurn(current))
                    return;
            }
        }

        private bool RunAiTurn(
            PlayerState player)
        {
            if (_engine.State.IsGameOver)
                return true;

            if (_engine.State.CurrentPlayerId !=
                player.Id)
            {
                return true;
            }

            /*
             * Trait selection هنوز بخشی از Setup است.
             * AI باید Trait خودش را انتخاب کند.
             */
            if (_engine.State.Phase ==
                GamePhase.TraitSelection)
            {
                if (player.TraitOptions.Count == 0)
                    return false;

                return _engine.SubmitAction(
                    GameAction.SelectTrait(
                        player.Id,
                        player.TraitOptions[0]));
            }

            GameAction action =
                _aiStrategy.DecideAction(
                    _engine.State,
                    player);

            if (action == null)
                return false;

            return _engine.SubmitAction(action);
        }
    }
}