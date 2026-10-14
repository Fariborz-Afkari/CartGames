using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeadManDraws
{
    public class PlayResult
    {
        public CardDefinition Card;
        public bool Kept;
        public bool Busted;
        public bool NeedsTarget;
        public int OraclePeekCount;
    }

    public class GameEngine
    {
        public GameData Data { get; private set; } = new GameData();
        public System.Random Rng = new System.Random();
        public bool PendingBust;
        public event Action<string> OnNotification;
        public event Action OnChanged;

        // Pending interaction flags
        public bool AwaitingCannonTarget;
        public bool AwaitingHookCard;
        public bool AwaitingMapChoice;
        public bool AwaitingSwordTarget;
        public bool AwaitingOracleDecision;

        public List<CardDefinition> PendingMapChoices = new List<CardDefinition>();

        public bool CanDrawMore = true;
        public int DrawnThisTurn = 0;
        public int KrakenForcedDraws = 0;

        public PlayerData CurrentPlayer => Data.Players[Data.CurrentPlayerIndex];
        public bool IsDeckEmpty => Data.DrawDeck.Count == 0;

        public void InitGame(int playerCount, int hardness, List<PlayerData> players, List<CardDefinition> cardPool)
        {
            Data.PlayerCount = playerCount;
            Data.Hardness = hardness;
            Data.Players = players;
            Data.AllPool = cardPool;
            Data.GameOver = false;

            BuildDecks(playerCount);
            Data.CurrentPlayerIndex = Rng.Next(players.Count);
            ResetTurnState();
        }

        void BuildDecks(int playerCount)
        {
            Data.DrawDeck.Clear();
            Data.BurnDeck.Clear();
            int copies = playerCount > 4 ? 2 : 1;

            var all = new List<CardDefinition>();
            for (int c = 0; c < copies; c++)
                foreach (var def in Data.AllPool)
                    all.Add(ScriptableObject.Instantiate(def));

            var grouped = new Dictionary<CardType, List<CardDefinition>>();
            foreach (var card in all)
            {
                if (!grouped.ContainsKey(card.CardType))
                    grouped[card.CardType] = new List<CardDefinition>();
                grouped[card.CardType].Add(card);
            }

            var remaining = new List<CardDefinition>();
            foreach (var kv in grouped)
            {
                var list = kv.Value;
                list.Sort((a, b) => a.Value.CompareTo(b.Value));
                int burnPerType = copies;
                for (int i = 0; i < burnPerType && i < list.Count; i++)
                    Data.BurnDeck.Add(list[i]);
                for (int i = burnPerType; i < list.Count; i++)
                    remaining.Add(list[i]);
            }

            Shuffle(remaining);
            Data.DrawDeck = remaining;
        }

        public void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public void ResetTurnState()
        {
            PendingBust = false;
            DrawnThisTurn = 0;
            KrakenForcedDraws = 0;
            CanDrawMore = true;
            AwaitingCannonTarget = AwaitingHookCard = AwaitingMapChoice =
                AwaitingSwordTarget = AwaitingOracleDecision = false;
            PendingMapChoices.Clear();

            foreach (var p in Data.Players)
            {
                p.Board.Clear();
                p.SafeDrawsRemaining = 0;
            }
        }

        public void Notify(string msg) => OnNotification?.Invoke(msg);
        public void RaiseChanged() => OnChanged?.Invoke();

        public CardDefinition DrawCard()
        {
            if (Data.DrawDeck.Count == 0) return null;
            int last = Data.DrawDeck.Count - 1;
            var c = Data.DrawDeck[last];
            Data.DrawDeck.RemoveAt(last);
            return c;
        }

        public CardDefinition PeekCard()
        {
            if (Data.DrawDeck.Count == 0) return null;
            return Data.DrawDeck[Data.DrawDeck.Count - 1];
        }

        public PlayResult PlayCardForCurrentPlayer(CardDefinition card)
        {
            var p = CurrentPlayer;
            var result = new PlayResult { Card = card };
            DrawnThisTurn++;

            p.Board.Add(card);

            if (HasDuplicateType(p.Board, card.CardType))
            {
                result.Busted = true;
                PendingBust = true;          // ← فقط علامت بزن، سوختن رو بعداً اجرا کن
                RaiseChanged();
                return result;
            }

            result.Kept = true;
            ExecuteAbility(p, card, result);
            RaiseChanged();
            return result;
        }
        public void ExecutePendingBust()
        {
            if (!PendingBust) return;
            PendingBust = false;

            var p = CurrentPlayer;
            if (p.Board.Count == 0) return;

            var causing = p.Board[p.Board.Count - 1];
            var result = new PlayResult { Card = causing, Busted = true };
            HandleBust(p, causing, result);
            RaiseChanged();
        }
        bool HasDuplicateType(List<CardDefinition> board, CardType t)
        {
            int n = 0;
            foreach (var c in board) if (c.CardType == t) n++;
            return n > 1;
        }
        void HandleBust(PlayerData p, CardDefinition causing, PlayResult result)
        {
            result.Busted = true;
            Notify($"{p.PlayerName} سوخت!");

            // پیدا کردن جای لنگر
            int anchorIdx = -1;
            for (int i = 0; i < p.Board.Count; i++)
                if (p.Board[i].CardType == CardType.Anchor) { anchorIdx = i; break; }

            bool safeHarbor = p.Trait == TraitType.SafeHarbor;

            // اگه لنگر نیست → همه کارت‌ها می‌سوزن (یا به Davy Jones می‌رن)
            if (anchorIdx < 0)
            {
                HandleNormalBust(p);
                return;
            }

            int lastIdx = p.Board.Count - 1;
            int lastSafe;

            if (safeHarbor)
                lastSafe = Mathf.Min(lastIdx - 1, anchorIdx + 2);   // لنگر + 2 کارت بعدش امن
            else
                lastSafe = anchorIdx - 1;                          // فقط کارت‌های قبل از لنگر امن

            // کارت‌های امن → بانک (به جز کارت باعث سوختن)
            for (int i = 0; i <= lastSafe; i++)
            {
                var c = p.Board[i];
                if (c == causing) continue;
                p.Bank.Add(c);
            }

            // کارت‌های ناامن + کارت باعث سوختن → دک سوخته
            for (int i = lastSafe + 1; i <= lastIdx; i++)
                Data.BurnDeck.Add(p.Board[i]);

            // اطمینان: کارت باعث سوختن همیشه می‌سوزه
            int causingIdx = p.Board.IndexOf(causing);
            if (causingIdx >= 0 && causingIdx <= lastSafe)
                Data.BurnDeck.Add(causing);

            p.Board.Clear();
            Notify("لنگر: فقط کارت‌های قبل از لنگر نجات یافتند.");
            RaiseChanged();
        }

        void HandleNormalBust(PlayerData p)
        {
            // Davy Jones Locker: کارت‌ها به بانک صاحب این trait می‌رن
            PlayerData davy = null;
            foreach (var other in Data.Players)
                if (other.Trait == TraitType.DavyJonesLocker && other.PlayerId == p.DavyJonesOwnerId)
                { davy = other; break; }

            if (davy != null)
            {
                foreach (var c in p.Board) davy.Bank.Add(c);
                p.Board.Clear();
                Notify($"صندوقچه دیوی جونز: کارت‌ها به بانک {davy.PlayerName} رفت.");
            }
            else
            {
                foreach (var c in p.Board) Data.BurnDeck.Add(c);
                p.Board.Clear();
            }
            RaiseChanged();
        }
        //void HandleBust(PlayerData p, CardDefinition causing, PlayResult result)
        //{
        //    result.Busted = true;
        //    Notify($"{p.PlayerName} سوخت!");

        //    bool anchorProtect = false;
        //    foreach (var c in p.Board) if (c.CardType == CardType.Anchor) { anchorProtect = true; break; }

        //    if (anchorProtect || p.Trait == TraitType.SafeHarbor)
        //    {
        //        // keep everything from anchor onwards, bank the rest
        //        var keep = new List<CardDefinition>();
        //        bool hitAnchor = false;
        //        foreach (var c in p.Board)
        //        {
        //            if (c.CardType == CardType.Anchor) hitAnchor = true;
        //            if (hitAnchor) keep.Add(c);
        //            else p.Bank.Add(c);
        //        }
        //        foreach (var c in keep) p.Bank.Add(c);
        //        p.Board.Clear();
        //        Notify("لنگر/لنگرگاه امن کارت‌های قبل را نجات داد.");
        //    }
        //    else
        //    {
        //        // Davy Jones Locker: owner of this player gets the board
        //        PlayerData davy = null;
        //        foreach (var other in Data.Players)
        //            if (other.Trait == TraitType.DavyJonesLocker && other.PlayerId == p.DavyJonesOwnerId)
        //            { davy = other; break; }

        //        if (davy != null)
        //        {
        //            foreach (var c in p.Board) davy.Bank.Add(c);
        //            p.Board.Clear();
        //            Notify($"صندوقچه دیوی جونز: کارت‌ها به بانک {davy.PlayerName} رفت.");
        //        }
        //        else
        //        {
        //            foreach (var c in p.Board) Data.BurnDeck.Add(c);
        //            p.Board.Clear();
        //        }
        //    }

        //    RaiseChanged();
        //}

        void ExecuteAbility(PlayerData p, CardDefinition card, PlayResult result)
        {
            switch (card.CardType)
            {
                case CardType.Anchor:
                    if (p.Trait == TraitType.SafeHarbor) p.SafeDrawsRemaining = 2;
                    break;

                case CardType.Cannon:
                    AwaitingCannonTarget = true;
                    result.NeedsTarget = true;
                    break;

                case CardType.Chest:
                case CardType.Key:
                    break; // resolved at collect time

                case CardType.Hook:
                    AwaitingHookCard = true;
                    result.NeedsTarget = true;
                    break;

                case CardType.Kraken:
                    if (p.Trait == TraitType.Fisherman)
                    {
                        p.Board.Remove(card);
                        p.Bank.Add(card);
                        Notify("ماهیگیر: کراکن فوراً بانک شد.");
                    }
                    else
                    {
                        int forced = 2;
                        if (AnyOpponentHasTrait(TraitType.Beastmaster)) forced = 4;
                        KrakenForcedDraws = forced;
                        Notify($"کراکن! باید {forced} کارت دیگر بکشی.");
                    }
                    break;

                case CardType.Map:
                    OpenMapChoices();
                    result.NeedsTarget = true;
                    break;

                case CardType.Mermaid:
                    if (p.Trait == TraitType.Casanova)
                    {
                        p.Board.Remove(card);
                        p.Bank.Add(card);
                        Notify("کازانووا: پری دریایی فوراً بانک شد.");
                    }
                    break;

                case CardType.Oracle:
                    result.OraclePeekCount = (p.Trait == TraitType.Mystic) ? 3 : 1;
                    AwaitingOracleDecision = true;
                    break;

                case CardType.Sword:
                    AwaitingSwordTarget = true;
                    result.NeedsTarget = true;
                    ResolveParry(p);
                    break;
            }
        }
        public List<CardDefinition> PeekCards(int count)
        {
            var result = new List<CardDefinition>();
            for (int i = 0; i < count; i++)
            {
                int idx = Data.DrawDeck.Count - 1 - i;
                if (idx < 0) break;
                result.Add(Data.DrawDeck[idx]);
            }
            return result;
        }

        public void CloseOracle()
        {
            AwaitingOracleDecision = false;
        }
        bool AnyOpponentHasTrait(TraitType t)
        {
            foreach (var p in Data.Players)
                if (p != CurrentPlayer && p.Trait == t) return true;
            return false;
        }

        // این فیلد رو کنار بقیه فیلدهای GameEngine اضافه کن
        public int MapCardCount = 3;

        void OpenMapChoices()
        {
            var p = CurrentPlayer;
            PendingMapChoices.Clear();

            if (Data.BurnDeck.Count == 0)
            {
                AwaitingMapChoice = false;
                Notify("نقشه: دک سوخته خالی است.");
                return;
            }

            Shuffle(Data.BurnDeck);

            int take;
            if (p.Trait == TraitType.Navigator)
                take = Data.BurnDeck.Count;                               // مسیریاب: همه
            else
                take = Mathf.Min(MapCardCount, Data.BurnDeck.Count);      // داینامیک

            for (int i = 0; i < take; i++)
                PendingMapChoices.Add(Data.BurnDeck[Data.BurnDeck.Count - 1 - i]);

            AwaitingMapChoice = true;
            Notify($"نقشه: یکی از {take} کارت را انتخاب کن.");
        }

        public void ResolveCannonCard(PlayerData target, CardDefinition card)
        {
            AwaitingCannonTarget = false;
            if (target == null || card == null) return;
            var p = CurrentPlayer;

            // Misfire: هدف نمی‌تونه انتخاب بشه، خودش بالاترین کارتش رو می‌سوزونه
            if (target.Trait == TraitType.Misfire)
            {
                var high = GetHighestFromBank(target);
                if (high != null) { target.Bank.Remove(high); Data.BurnDeck.Add(high); }
                RaiseChanged();
                return;
            }

            // Master Gunner: کل کارت‌های هم‌نوع رو می‌سوزونه
            if (p.Trait == TraitType.MasterGunner)
            {
                var all = target.Bank.FindAll(c => c.CardType == card.CardType);
                foreach (var c in all) { target.Bank.Remove(c); Data.BurnDeck.Add(c); }
            }
            else
            {
                target.Bank.Remove(card);
                if (p.Trait == TraitType.Scavenger) p.Bank.Add(card);
                else Data.BurnDeck.Add(card);
            }
            RaiseChanged();
        }

        public void ResolveSwordCard(PlayerData target, CardDefinition card)
        {
            AwaitingSwordTarget = false;
            if (target == null || card == null) return;
            target.Bank.Remove(card);
            PlayCardForCurrentPlayer(card);
        }

        CardDefinition GetHighestFromBank(PlayerData p)
        {
            CardDefinition best = null;
            foreach (var c in p.Bank) if (best == null || c.Value > best.Value) best = c;
            return best;
        }

        public void ResolveMapChoice(CardDefinition chosen)
        {
            AwaitingMapChoice = false;
            PendingMapChoices.Clear();
            if (chosen == null) return;

            Data.BurnDeck.Remove(chosen);
            PlayCardForCurrentPlayer(chosen);
        }

        public void ResolveHookSelection(CardDefinition card)
        {
            AwaitingHookCard = false;
            if (card == null) return;

            var p = CurrentPlayer;
            p.Bank.Remove(card);
            p.Board.Add(card);

            // مثل کارت کشیدن: چک تکراری بودن
            if (HasDuplicateType(p.Board, card.CardType))
            {
                PendingBust = true;
                RaiseChanged();
                return;
            }

            var res = new PlayResult { Card = card, Kept = true };
            ExecuteAbility(p, card, res);
            RaiseChanged();
        }


        void ResolveParry(PlayerData swordUser)
        {
            foreach (var p in Data.Players)
            {
                if (p.Trait != TraitType.Parry || p == swordUser) continue;
                var kraken = swordUser.Bank.Find(c => c.CardType == CardType.Kraken);
                if (kraken != null)
                {
                    swordUser.Bank.Remove(kraken);
                    swordUser.Board.Add(kraken);
                    Notify("دفع حمله: کراکن به میدان آمد!");
                }
            }
        }

        public void ResolveOracleDecision(bool cont)
        {
            AwaitingOracleDecision = false;
            CanDrawMore = cont;
        }

        public void OnPlayerCollect()
        {
            var p = CurrentPlayer;
            bool hasChest = p.Board.Exists(c => c.CardType == CardType.Chest);
            bool hasKey = p.Board.Exists(c => c.CardType == CardType.Key);

            if (hasChest && hasKey)
            {
                int cnt = p.Board.Count;
                if (p.Trait == TraitType.Plunderer)
                {
                    foreach (var other in Data.Players)
                    {
                        if (other == p) continue;
                        int take = Mathf.Min(cnt, other.Bank.Count);
                        for (int i = 0; i < take; i++)
                        {
                            p.Bank.Add(other.Bank[0]);
                            other.Bank.RemoveAt(0);
                        }
                        break;
                    }
                }
                else
                {
                    Shuffle(Data.BurnDeck);
                    int take = Mathf.Min(cnt, Data.BurnDeck.Count);
                    for (int i = 0; i < take; i++)
                    {
                        int last = Data.BurnDeck.Count - 1;
                        p.Bank.Add(Data.BurnDeck[last]);
                        Data.BurnDeck.RemoveAt(last);
                    }
                }
            }

            if (p.Trait == TraitType.TreasureHunter &&
                p.HasCardInBank(CardType.Chest) && p.HasCardInBank(CardType.Key))
            {
                int c = p.Board.Count * 3;
                Shuffle(Data.BurnDeck);
                int take = Mathf.Min(c, Data.BurnDeck.Count);
                for (int i = 0; i < take; i++)
                {
                    int last = Data.BurnDeck.Count - 1;
                    p.Bank.Add(Data.BurnDeck[last]);
                    Data.BurnDeck.RemoveAt(last);
                }
            }

            foreach (var c in p.Board) p.Bank.Add(c);
            p.Board.Clear();
            RaiseChanged();
        }

        public void NextTurn()
        {
            Data.CurrentPlayerIndex = (Data.CurrentPlayerIndex + 1) % Data.Players.Count;
            ResetTurnState();
            RaiseChanged();
        }

        public bool CheckGameOver()
        {
            if (Data.DrawDeck.Count == 0)
            {
                Data.GameOver = true;
                return true;
            }
            return false;
        }

        public PlayerData GetWinner()
        {
            PlayerData best = null;
            foreach (var p in Data.Players)
            {
                if (best == null) { best = p; continue; }
                int s1 = p.ComputeScore(), s2 = best.ComputeScore();
                if (s1 > s2 || (s1 == s2 && p.Bank.Count > best.Bank.Count))
                    best = p;
            }
            return best;
        }
    }
}