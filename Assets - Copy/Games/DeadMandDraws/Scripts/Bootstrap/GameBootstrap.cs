using UnityEngine;
using CardGames.DeadManDraws.Presentation.UI;

namespace CardGames.DeadManDraws.Bootstrap
{
    /// <summary>
    /// Unity composition root for the game scene.
    ///
    /// Bootstrap is intentionally unaware of:
    /// - GamePresenter
    /// - GameViewModel
    /// - GameEngine
    /// - Core game rules
    ///
    /// Its only responsibility is to create the presentation root.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField]
        private GameUi _gameUiPrefab;

        private GameUi _gameUi;

        private void Awake()
        {
            if (_gameUiPrefab == null)
            {
                Debug.LogError(
                    "GameBootstrap: GameUi prefab is not assigned.");

                return;
            }

            _gameUi = Instantiate(
                _gameUiPrefab);

            _gameUi.name = "GameUI";
        }
    }
}
/* 
 Scene
  │
  ▼
GameBootstrap
  │
  ▼
GameUi
  │
  ▼
GameViewModel
  │
  ▼
GamePresenter
  │
  ▼
GameEngine
 */
