using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AngelVSZombie
{
    public class GameEngine
    {
        public GameData Data;
        public GameUIController UI;
        public GameFlowController Flow;

        public GameEngine(GameData data, GameUIController ui, GameFlowController flow)
        {
            Data = data;
            UI = ui;
            Flow = flow;
        }

        public void SetupGame(List<PlayerDefinition> playerDefs, List<CardDefinition> allCards)
        {
            Data.Players.Clear();
            foreach (var def in playerDefs)
            {
                Data.Players.Add(new PlayerData(def));
            }

            var deck = new List<CardDefinition>(allCards);

            var zombies = deck.Where(c => c.CardType == CardType.Zombie).ToList();
            var angels = deck.Where(c => c.CardType == CardType.Angel).ToList();
            var others = deck.Where(c => c.CardType != CardType.Zombie && c.CardType != CardType.Angel).ToList();

            Data.Deck.Clear();
            Data.Deck.AddRange(others);

            // ابتدا کارت‌های غیرزامبی را شافل می‌کنیم تا بازیکنان زامبی نگیرند
            ShuffleDeck();

            // پخش 7 کارت به هر بازیکن
            foreach (var player in Data.Players)
            {
                for (int i = 0; i < 7; i++)
                {
                    player.Hand.Add(Data.Deck[0]);
                    Data.Deck.RemoveAt(0);
                }

                // دادن 1 فرشته به هر بازیکن
                if (angels.Count > 0)
                {
                    player.Hand.Add(angels[0]);
                    angels.RemoveAt(0);
                    player.HasAngel = true;
                }
            }

            // حالا زامبی‌ها و فرشته‌های اضافی را به دک اضافه می‌کنیم
            int zombieCount = Data.Players.Count - 1;
            for (int i = 0; i < zombieCount && i < zombies.Count; i++)
            {
                Data.Deck.Add(zombies[i]);
            }

            int extraAngelCount = Data.Players.Count == 4 ? 1 : 0;
            for (int i = 0; i < extraAngelCount && i < angels.Count; i++)
            {
                Data.Deck.Add(angels[i]);
            }

            ShuffleDeck();
            UI.UpdateDeckCount(Data.Deck.Count);
        }

        // متد کشیدن کارت حالا یک bool برمی‌گرداند تا به کنترلر بگوید آیا زامبی کشیده شده یا نه
        public bool DrawCardFromDeck(PlayerData player)
        {
            if (Data.Deck.Count == 0)
            {
                UI.UpdateStatus("دک خالی است!");
                return false;
            }

            CardDefinition drawn = Data.Deck[0];
            Data.Deck.RemoveAt(0);
            UI.UpdateDeckCount(Data.Deck.Count);

            if (drawn.CardType == CardType.Zombie)
            {
                // اگر زامبی بود، به دست اضافه نمی‌شود
                Flow.HandleZombieDraw(player, drawn);
                return true;
            }
            else
            {
                // اگر کارت معمولی بود، به دست اضافه می‌شود
                player.Hand.Add(drawn);
                return false;
            }
        }

        public void ShuffleDeck()
        {
            for (int i = 0; i < Data.Deck.Count; i++)
            {
                CardDefinition temp = Data.Deck[i];
                int randomIndex = Random.Range(i, Data.Deck.Count);
                Data.Deck[i] = Data.Deck[randomIndex];
                Data.Deck[randomIndex] = temp;
            }
        }

        
        public void HandleZombieDraw(PlayerData player)
        {
            if (player.HasAngel)
            {
                UI.UpdateStatus($"{player.Definition.PlayerName} زامبی کشید! آیا از فرشته استفاده می‌کند؟");
                Flow.PromptUseAngel(player);
            }
            else
            {
                player.IsAlive = false;
                UI.UpdateStatus($"{player.Definition.PlayerName} مرد!");
                UI.RefreshPlayerPanels(); // این خط اضافه شود
                Flow.CheckWinCondition();
            }
        }

        public void PlayCards(PlayerData player, List<CardDefinition> cards)
        {
            foreach (var card in cards)
            {
                player.Hand.Remove(card);
                Data.DiscardPile.Add(card);
            }

            if (cards.Count >= 2 && cards.Count <= 3)
            {
                bool allSame = cards.All(c => c.CardType == cards[0].CardType);
                if (allSame || (cards.Count(c => c.CardType != CardType.Clone) == 1 && cards.Any(c => c.CardType == CardType.Clone)))
                {
                    HandleMatchingCards(player, cards);
                    return;
                }
            }

            if (cards.Count == 1)
            {
                ResolveCardEffect(player, cards[0]);
            }
        }

        private void HandleMatchingCards(PlayerData player, List<CardDefinition> cards)
        {
            UI.UpdateStatus($"{player.Definition.PlayerName} {cards.Count} کارت همسان بازی کرد! افکت آن‌ها باطل شد.");

            if (cards.Count == 2)
            {
                UI.UpdateStatus("می‌توانید یک بازیکن را انتخاب کرده و بطور شانسی یک کارت او را بردارید.");
                Flow.PromptRandomSteal(); // متد جدید در Flow برای این کار
            }
            else if (cards.Count == 3)
            {
                UI.UpdateStatus("می‌توانید یک بازیکن را انتخاب کرده و یک کارت مشخص از او بخواهید.");
                Flow.PromptSpecificSteal(); // متد جدید در Flow برای این کار
            }
        }

        public void ResolveCardEffect(PlayerData player, CardDefinition card)
        {
            Data.LastPlayedCard = card;
            Data.LastPlayedPlayer = player;

            switch (card.CardType)
            {
                case CardType.Attack:
                    Data.ExtraTurns += 1;
                    UI.UpdateStatus("حمله! بازیکن بعدی یک نوبت اضافه بازی می‌کند.");
                    break;

                case CardType.Nope:
                    if (Data.LastPlayedCard != null)
                    {
                        UI.UpdateStatus($"{Data.LastPlayedPlayer.Definition.PlayerName} کارت {Data.LastPlayedCard.CardName} او باطل شد!");
                        Data.LastPlayedCard = null;
                        Data.LastPlayedPlayer = null;
                    }
                    break;

                case CardType.Eye:
                    var topCard = PeekDeck(1);
                    if (topCard.Count > 0)
                    {
                        Flow.PromptEyeEffect(topCard[0]);
                    }
                    break;

                case CardType.Zebel:
                    Data.SkipDraw = true;
                    UI.UpdateStatus("زبل! نوبت شما بدون کارت کشیدن به پایان می‌رسد.");
                    break;

                case CardType.ZebelKhan:
                    Data.SkipDraw = true;
                    Data.ExtraTurns = 0;
                    UI.UpdateStatus("زبل خان! نوبت به بازیکن بعدی منتقل شد و نوبت‌های اضافه باطل شدند.");
                    break;

                case CardType.Oracle:
                    var top3 = PeekDeck(3);
                    Flow.PromptOracleEffect(top3);
                    break;

                case CardType.Shuffle:
                    ShuffleDeck();
                    UI.UpdateStatus("دک بر زده شد!");
                    break;

                case CardType.FeedDead:
                    Flow.PromptFeedDeadSelection();
                    break;

                case CardType.Clone:
                    // بررسی می‌کنیم که آیا کارتی برای کپی کردن وجود دارد یا نه
                    // (چون کارت کپی جدید الان به دک سوخته اضافه شده، باید یکی عقب‌تر را نگاه کنیم)
                    if (Data.DiscardPile.Count >= 2)
                    {
                        CardDefinition cardToCopy = Data.DiscardPile[Data.DiscardPile.Count - 2];

                        if (cardToCopy.CardType == CardType.Clone)
                        {
                            UI.UpdateStatus("کپی نامعتبر! (نمی‌توان روی کپی، کپی کرد)");
                        }
                        else
                        {
                            UI.UpdateStatus($"کپی! افکت کارت {cardToCopy.CardName} تکرار می‌شود.");
                            ResolveCardEffect(player, cardToCopy);
                        }
                    }
                    else
                    {
                        UI.UpdateStatus("کپی! هیچ کارتی برای کپی کردن در دک سوخته وجود ندارد.");
                    }
                    break;
                case CardType.GraveRubber:
                    foreach (var dead in Data.Players.Where(p => !p.IsAlive))
                    {
                        if (dead.Hand.Count > 0)
                        {
                            Data.Deck.Add(dead.Hand[0]);
                            dead.Hand.RemoveAt(0);
                        }
                    }
                    ShuffleDeck();
                    foreach (var dead in Data.Players.Where(p => !p.IsAlive))
                    {
                        DrawCardFromDeck(dead);
                    }
                    UI.UpdateStatus("مرده خوری! همه مرده‌ها کارت عوض کردند.");
                    break;

                case CardType.Gift:
                    Flow.PromptGiftSelection();
                    break;

                case CardType.Tunnel:
                    var tunnelCard = PeekDeck(1);
                    if (tunnelCard.Count > 0)
                    {
                        Flow.PromptTunnelEffect(tunnelCard[0]);
                    }
                    break;

                case CardType.DeadsAttack:
                    int deadCount = Data.Players.Count(p => !p.IsAlive);
                    if (deadCount > 0)
                    {
                        Data.ExtraTurns += 3 * deadCount;
                        Data.SkipDraw = true;
                        UI.UpdateStatus($"حمله ارواح! بازیکن بعدی باید {3 * deadCount} نوبت بازی کند.");
                    }
                    break;

                default:
                    UI.UpdateStatus($"کارت {card.CardName} بازی شد.");
                    break;
            }
        }

        public List<CardDefinition> PeekDeck(int count)
        {
            List<CardDefinition> cards = new List<CardDefinition>();
            for (int i = 0; i < count && i < Data.Deck.Count; i++)
            {
                cards.Add(Data.Deck[i]);
            }
            return cards;
        }
    }
}