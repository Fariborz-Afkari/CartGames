namespace DeadManDraws
{
    [System.Serializable]
    public class PlayerDefinition
    {
        public int PlayerId;
        public string PlayerName;
        public int AvatarIndex;
        public bool IsHuman;
        public int Score;
        public int BankCardCount;
        public TraitType Trait;
    }
}