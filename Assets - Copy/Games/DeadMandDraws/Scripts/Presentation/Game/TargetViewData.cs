namespace CardGames.DeadManDraws.Presentation.Game
{
    public sealed class TargetViewData
    {
        public int PlayerId { get; }
        public string PlayerName { get; }

        public TargetViewData(
            int playerId,
            string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
    }
}
/*
 بله. طبق معماری نهایی، TargetViewData هم باید یک مدل کاملاً مستقل و فقط مخصوص Presentation باشد.
قرارداد نهایی
PlayerId → شناسه بازیکنی که UI باید برای انتخاب به کاربر معرفی کند.
PlayerName → نام نمایشی بازیکن.
propertyها فقط خواندنی هستند.
هیچ وابستگی به Core، Unity، GamePresenter یا GameViewModel ندارد.
هیچ منطق انتخاب یا اعتبارسنجی داخل آن نیست.

جریان استفاده:

GameEngine / GameState
        ↓
GamePresenter
        ↓
TargetViewData
        ↓
CardInteraction
        ↓
GameViewModel
        ↓
GameUi

این کلاس در همین شکل نهایی است و نیازی به پیچیده‌تر کردن آن نداریم.
 */