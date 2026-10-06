using System;
using System.Collections.Generic;
using CardGames.DeadManDraws.Core.Players;
using UnityEngine;

namespace CardGames.DeadManDraws.Presentation.Game
{
    public sealed class GameViewModel : IDisposable
    {
        private readonly GamePresenter _presenter;

        public event Action Changed;

        public GameViewModel()
        {
            _presenter =
                new GamePresenter();

            _presenter.Changed +=
                OnPresenterChanged;
        }

        public int Coins
        {
            get { return _presenter.Coins; }
        }

        public int Score
        {
            get { return _presenter.Score; }
        }

        public bool CanStartMatch
        {
            get { return _presenter.CanStartMatch; }
        }

        public bool IsPlayerTurn
        {
            get { return _presenter.IsPlayerTurn; }
        }

        public bool IsTraitSelection
        {
            get { return _presenter.IsTraitSelection; }
        }

        public bool IsGameOver
        {
            get { return _presenter.IsGameOver; }
        }

        public int? WinnerId
        {
            get { return _presenter.WinnerId; }
        }

        public int CurrentPlayerId
        {
            get { return _presenter.CurrentPlayerId; }
        }

        public string CurrentPlayerName
        {
            get { return _presenter.CurrentPlayerName; }
        }

        public string Status
        {
            get { return _presenter.Status; }
        }

        public int TurnNumber
        {
            get { return _presenter.TurnNumber; }
        }

        public int DeckCount
        {
            get { return _presenter.DeckCount; }
        }

        public int DiscardCount
        {
            get { return _presenter.DiscardCount; }
        }

        public PlayerTrait HumanTrait
        {
            get { return _presenter.HumanTrait; }
        }

        public IReadOnlyList<PlayerTrait> TraitOptions
        {
            get
            {
                return _presenter.GetHumanTraitOptions();
            }
        }

        public IReadOnlyList<CardViewData> Hand
        {
            get
            {
                return _presenter.GetPlayerHand();
            }
        }

        public IReadOnlyList<CardViewData> PlayArea
        {
            get
            {
                return _presenter.GetPlayerPlayArea();
            }
        }

        public IReadOnlyList<CardViewData> Bank
        {
            get
            {
                return _presenter.GetPlayerBank();
            }
        }

        public IReadOnlyList<PlayerViewData> Players
        {
            get
            {
                return _presenter.GetPlayers();
            }
        }

        public IReadOnlyList<string> TurnLog
        {
            get { return _presenter.TurnLog; }
        }

        // ============================================================
        // MATCH
        // ============================================================

        public void StartMatch(
            int playerCount,
            int aiDifficulty)
        {
            _presenter.StartMatch(
                playerCount,
                aiDifficulty);
        }

        // ============================================================
        // TRAIT
        // ============================================================

        public bool SelectTrait(
            PlayerTrait trait)
        {
            return _presenter.SelectTrait(
                trait);
        }

        // ============================================================
        // GAME ACTIONS
        // ============================================================

        public bool DrawCard()
        {
            return _presenter.DrawCard();
        }

        public bool StopDrawing()
        {
            return _presenter.StopDrawing();
        }

        public bool PlayCard(
            int cardId,
            int? targetPlayerId = null)
        {
            return _presenter.PlayCard(
                cardId,
                targetPlayerId);
        }

        public void EndTurn()
        {
            _presenter.EndTurn();
        }

        public void BuyCoins()
        {
            _presenter.BuyCoins();
        }

        public CardInteraction GetCardInteraction(
            int cardId)
        {
            return _presenter.GetCardInteraction(
                cardId);
        }

        // ============================================================
        // DISPOSE
        // ============================================================

        public void Dispose()
        {
            if (_presenter != null)
            {
                _presenter.Changed -=
                    OnPresenterChanged;

                _presenter.Dispose();
            }
        }

        private void OnPresenterChanged()
        {
            Changed?.Invoke();
        }
    }
}