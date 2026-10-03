using System.Collections.Generic;

namespace CardGames.DeadManDraws.Presentation.Game
{
    public sealed class CardInteraction
    {
        public int CardId { get; }
        public bool RequiresTarget { get; }
        public IReadOnlyList<TargetViewData> Targets { get; }

        public CardInteraction(
            int cardId,
            bool requiresTarget,
            IReadOnlyList<TargetViewData> targets)
        {
            CardId = cardId;
            RequiresTarget = requiresTarget;
            Targets = targets;
        }
    }
}
/* 
 طبق معماری نهایی، CardInteraction باید فقط نتیجه‌ی قابل‌مصرف برای UI را توصیف کند؛ یعنی UI بداند بعد از انتخاب یک کارت، آیا target لازم است و چه targetهایی مجازند.

قرارداد نهایی
CardInteraction
├── CardId
├── RequiresTarget
└── Targets
      └── TargetViewData[]

نکات معماری:

CardId همان شناسه‌ای است که بعداً به PlayCard() داده می‌شود.
RequiresTarget مشخص می‌کند UI باید پنل انتخاب هدف را نمایش دهد یا نه.
Targets فقط داده‌ی نمایشی targetهاست.
CardInteraction هیچ منطق gameplay ندارد.
هیچ وابستگی به GamePresenter، GameViewModel، GameEngine یا Unity ندارد.
TargetViewData را مصرف می‌کند، ولی خودش مسئول ساخت آن نیست.
IReadOnlyList باعث می‌شود مصرف‌کننده نتواند لیست targetها را از طریق این API تغییر دهد.

در نتیجه CardInteraction هم با معماری نهایی کاملاً مستقل است و بعد از این نباید به nested class داخل GamePresenter برگردد.
 */
