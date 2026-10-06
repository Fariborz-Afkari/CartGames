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

        // ============================================================
        // TRAIT SELECTION
        // ============================================================

        public void CompleteAiTraitSelections()
        {
            if (_engine.State.IsGameOver)
                return;

            if (_engine.State.Phase !=
                GamePhase.TraitSelection)
            {
                return;
            }

            for (int i = 0;
                 i < _engine.State.Players.Count;
                 i++)
            {
                PlayerState player =
                    _engine.State.Players[i];

                if (player == null)
                    continue;

                if (player.IsHuman)
                    continue;

                if (player.Trait != PlayerTrait.None)
                    continue;

                if (player.TraitOptions == null ||
                    player.TraitOptions.Count == 0)
                {
                    continue;
                }

                _engine.SubmitAction(
                    GameAction.SelectTrait(
                        player.Id,
                        player.TraitOptions[0]));
            }
        }

        // ============================================================
        // AI TURNS
        // ============================================================

        public void RunAiTurns()
        {
            // --------------------------------------------------------
            // اول Trait تمام AIها را کامل کن.
            // --------------------------------------------------------

            if (_engine.State.Phase ==
                GamePhase.TraitSelection)
            {
                CompleteAiTraitSelections();

                // اگر هنوز Trait انتخاب نشده باشد
                // امکان شروع Turn وجود ندارد.
                if (_engine.State.Phase ==
                    GamePhase.TraitSelection)
                {
                    return;
                }
            }

            // --------------------------------------------------------
            // حالا Turnهای AI
            // --------------------------------------------------------

            while (!_engine.State.IsGameOver)
            {
                PlayerState current =
                    _engine.State.FindPlayer(
                        _engine.State.CurrentPlayerId);

                if (current == null)
                    return;

                // نوبت بازیکن انسانی است.
                if (current.IsHuman)
                    return;

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

            if (_engine.State.Phase ==
                GamePhase.TraitSelection)
            {
                if (player.TraitOptions == null ||
                    player.TraitOptions.Count == 0)
                {
                    return false;
                }

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