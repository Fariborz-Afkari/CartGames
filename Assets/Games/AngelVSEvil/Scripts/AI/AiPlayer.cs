using System.Linq;
using UnityEngine;

namespace AngelVSZombie
{
    public class AiPlayer
    {
        public PlayerData Player;
        private GameEngine _engine;
        private int _hardness; // 1=Easy, 2=Normal, 3=Hard

        public AiPlayer(PlayerData player, GameEngine engine, int hardness = 2)
        {
            Player = player;
            _engine = engine;
            _hardness = hardness;
        }

        public CardDefinition DecideCardToPlay()
        {
            if (Player.Hand.Count == 0) return null;

            switch (_hardness)
            {
                case 1: // Easy: گاهی بازی نمی‌کند، وگرنه کارت تصادفی
                    if (Random.value < 0.5f) return null;
                    return Player.Hand[Random.Range(0, Player.Hand.Count)];

                case 3: // Hard: هوشمندترین رفتار
                    return DecideHard();

                case 2: // Normal
                default:
                    return DecideNormal();
            }
        }

        private CardDefinition DecideNormal()
        {
            // اولویت: حمله -> برزدن
            var attack = Player.Hand.FirstOrDefault(c => c.CardType == CardType.Attack);
            if (attack != null) return attack;

            var shuffle = Player.Hand.FirstOrDefault(c => c.CardType == CardType.Shuffle);
            if (shuffle != null) return shuffle;

            return null;
        }

        private CardDefinition DecideHard()
        {
            // اولویت: حمله -> حمله ارواح -> برزدن -> زبل خان
            var attack = Player.Hand.FirstOrDefault(c => c.CardType == CardType.Attack);
            if (attack != null) return attack;

            // اگر بازیکن مرده‌ای وجود دارد، حمله ارواح بزن
            if (_engine.Data.Players.Any(p => !p.IsAlive))
            {
                var deadsAttack = Player.Hand.FirstOrDefault(c => c.CardType == CardType.DeadsAttack);
                if (deadsAttack != null) return deadsAttack;
            }

            var shuffle = Player.Hand.FirstOrDefault(c => c.CardType == CardType.Shuffle);
            if (shuffle != null) return shuffle;

            var zebelKhan = Player.Hand.FirstOrDefault(c => c.CardType == CardType.ZebelKhan);
            if (zebelKhan != null) return zebelKhan;

            return null;
        }

        /// <summary>
        /// تصمیم‌گیری برای استفاده از فرشته هنگام کشیدن زامبی
        /// </summary>
        public bool DecideUseAngel()
        {
            // اگر فرشته‌ای در دست ندارد، حتماً نه
            if (!Player.Hand.Any(c => c.CardType == CardType.Angel)) return false;

            switch (_hardness)
            {
                case 1: // آسان: 30٪ شانس استفاده
                    return Random.value < 0.3f;
                case 3: // سخت: همیشه استفاده می‌کند
                    return true;
                case 2: // معمولی: 70٪ شانس استفاده
                default:
                    return Random.value < 0.7f;
            }
        }
    }
}