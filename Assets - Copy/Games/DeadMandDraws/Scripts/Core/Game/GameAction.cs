using CardGames.DeadManDraws.Core.Players;
namespace CardGames.DeadManDraws.Core.Game
{
    public enum GameActionType
    {
        DrawCard,
        StopDrawing,
        BankPlayArea,

        SelectTrait,

        SelectCannonTarget,
        SelectCannonCard,

        SelectHookCard,

        SelectSwordTarget,
        SelectSwordCard,

        SelectMapCard,

        SelectPlundererTarget,

        SelectDavyJonesTarget
    }

    public sealed class GameAction
    {
        public GameActionType Type { get; }

        public int PlayerId { get; }

        public int? CardId { get; }

        public int? TargetPlayerId { get; }

        public int? TargetCardId { get; }

        public PlayerTrait? Trait { get; }

        private GameAction(
            GameActionType type,
            int playerId,
            int? cardId = null,
            int? targetPlayerId = null,
            int? targetCardId = null,
            PlayerTrait? trait = null)
        {
            Type = type;
            PlayerId = playerId;
            CardId = cardId;
            TargetPlayerId = targetPlayerId;
            TargetCardId = targetCardId;
            Trait = trait;
        }

        public static GameAction DrawCard(
            int playerId)
        {
            return new GameAction(
                GameActionType.DrawCard,
                playerId);
        }

        public static GameAction StopDrawing(
            int playerId)
        {
            return new GameAction(
                GameActionType.StopDrawing,
                playerId);
        }

        public static GameAction BankPlayArea(
            int playerId)
        {
            return new GameAction(
                GameActionType.BankPlayArea,
                playerId);
        }

        public static GameAction SelectTrait(
            int playerId,
            PlayerTrait trait)
        {
            return new GameAction(
                GameActionType.SelectTrait,
                playerId,
                trait: trait);
        }

        public static GameAction Cannon(
            int playerId,
            int targetPlayerId)
        {
            return new GameAction(
                GameActionType.SelectCannonTarget,
                playerId,
                targetPlayerId:
                    targetPlayerId);
        }

        public static GameAction CannonCard(
            int playerId,
            int targetPlayerId,
            int targetCardId)
        {
            return new GameAction(
                GameActionType.SelectCannonCard,
                playerId,
                targetPlayerId:
                    targetPlayerId,
                targetCardId:
                    targetCardId);
        }

        public static GameAction Hook(
            int playerId,
            int cardId)
        {
            return new GameAction(
                GameActionType.SelectHookCard,
                playerId,
                cardId);
        }

        public static GameAction Sword(
            int playerId,
            int targetPlayerId,
            int targetCardId)
        {
            return new GameAction(
                GameActionType.SelectSwordCard,
                playerId,
                targetPlayerId:
                    targetPlayerId,
                targetCardId:
                    targetCardId);
        }

        public static GameAction Map(
            int playerId,
            int cardId)
        {
            return new GameAction(
                GameActionType.SelectMapCard,
                playerId,
                cardId);
        }

        public static GameAction PlundererTarget(
            int playerId,
            int targetPlayerId)
        {
            return new GameAction(
                GameActionType.SelectPlundererTarget,
                playerId,
                targetPlayerId:
                    targetPlayerId);
        }

        public static GameAction DavyJonesTarget(
            int playerId,
            int targetPlayerId)
        {
            return new GameAction(
                GameActionType.SelectDavyJonesTarget,
                playerId,
                targetPlayerId:
                    targetPlayerId);
        }
    }
}