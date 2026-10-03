using System.Collections.Generic;
using CardGames.DeadManDraws.Core.Cards;

namespace CardGames.DeadManDraws.Core.Players
{
    public sealed class PlayerState
    {
        public int Id { get; }

        public string Name { get; }

        public bool IsHuman { get; }

        public CardCollection Bank { get; }

        public CardCollection PlayArea { get; }

        public PlayerTrait Trait { get; internal set; }

        public IReadOnlyList<PlayerTrait>TraitOptions
        {
            get { return _traitOptions; }
        }

        public int? DavyJonesTargetId
        {
            get;
            internal set;
        }

        public int? PlundererTargetId
        {
            get;
            internal set;
        }

        public int ProtectedPlayCards { get; internal set; }

        internal int SafeHarborCardsLeft { get; set; }

        public bool HasDrawnAtLeastOne { get; internal set; }

        internal bool IsTurnEnding { get; set; }

        private readonly List<PlayerTrait>_traitOptions;

        public PlayerState(
            int id,
            string name,
            bool isHuman)
        {
            Id = id;
            Name = name;
            IsHuman = isHuman;

            Bank = new CardCollection();
            PlayArea = new CardCollection();

            Trait = PlayerTrait.None;

            _traitOptions =
                new List<PlayerTrait>();
        }

        internal void SetTraitOptions(
            PlayerTrait first,
            PlayerTrait second)
        {
            _traitOptions.Clear();

            _traitOptions.Add(first);
            _traitOptions.Add(second);
        }

        internal void ClearTurnState()
        {
            ProtectedPlayCards = 0;
            SafeHarborCardsLeft = 0;
            HasDrawnAtLeastOne = false;
            IsTurnEnding = false;
        }

        public bool HasTrait(
            PlayerTrait trait)
        {
            return Trait == trait;
        }
    }
}