namespace AngelVSZombie
{
    public enum GameState
    {
        Setup,
        PlayerTurn,
        WaitingForAction, // زمانی که منتظر انتخاب کارت یا بازیکن هستیم (مثل هدیه)
        AiTurn,
        GameOver
    }
}