namespace HavenAttackModMath
{
    public abstract class AttackDeckSimulator<TEngine> where TEngine : BaseEngine
    {
        protected readonly List<Card> Cards;
        protected readonly TEngine Engine;

        public AttackDeckSimulator(IEnumerable<CardGroup> cardValues)
        {
            Cards = cardValues.SelectMany(cg =>
                Enumerable.Repeat(cg, cg.Count)
                    .Select(Card.FromCardGroup))
                .ToList();
            Engine = (TEngine)(Activator.CreateInstance(typeof(TEngine), args: [Cards]) ?? throw new ArgumentException());
        }

        public virtual Card DrawCard()
        {
            return Engine.DrawCard();
        }

        public virtual void Shuffle()
        {
            Engine.Shuffle();
        }

        protected abstract IEnumerable<Card> DrawVantage();

        protected virtual IEnumerable<Card> DrawNormal()
        {
            Card card;

            do
            {
                card = DrawCard();
                yield return card;
            } while (card.Rolling);
        }

        public virtual IEnumerable<Card> DrawCards(AttackKind kind)
        {
            return kind switch
            {
                AttackKind.Normal => DrawNormal(),
                AttackKind.Advantage => DrawVantage(),
                AttackKind.Disadvantage => DrawVantage(),
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported value")
            };
        }

        protected abstract Card GetResultCard(AttackKind kind, IReadOnlyList<Card> cards);

        public virtual Card Attack(AttackKind kind)
        {
            var cards = DrawCards(kind).ToList();
            if (cards.Any(c => c.Terminal))
            {
                Shuffle();
            }

            return GetResultCard(kind, cards);
        }

        public virtual Card Attack(AttackKind kind, out IReadOnlyList<Card> cards)
        {
            cards = DrawCards(kind).ToList();
            if (cards.Any(c => c.Terminal))
            {
                Shuffle();
            }

            return GetResultCard(kind, cards);
        }
    }

    public class GloomhavenAttackDeckSimulator<TEngine>(IEnumerable<CardGroup> cardValues) : AttackDeckSimulator<TEngine>(cardValues) where TEngine : BaseEngine
    {
        protected override IEnumerable<Card> DrawVantage()
        {
            bool nonRollingFound = false;
            int cards = 0;
            Card card;
            while (cards < 2 || !nonRollingFound)
            {
                card = DrawCard();
                yield return card;
                cards++;
                nonRollingFound |= !card.Rolling;
            }
        }

        protected override Card GetResultCard(AttackKind kind, IReadOnlyList<Card> cards)
        {
            if (kind == AttackKind.Normal)
            {
                return cards.First(c => !c.Rolling);
            }

            if (cards.Take(2).All(c => c.Rolling))
            {
                return cards.SkipWhile(c => c.Rolling).First();
            }

            return kind switch
            {
                AttackKind.Advantage => cards.Take(2).Where(c => !c.Rolling).MaxBy(c => c.Value) ?? cards.First(),
                AttackKind.Disadvantage => cards.Take(2).Where(c => !c.Rolling).MinBy(c => c.Value) ?? cards.First(),
                _ => throw new NotImplementedException()
            };
        }
    }

    public class FrosthavenAttackDeckSimulator<TEngine>(IEnumerable<CardGroup> cardValues) : AttackDeckSimulator<TEngine>(cardValues) where TEngine : BaseEngine
    {
        protected override IEnumerable<Card> DrawVantage()
        {
            Card card;

            // First card(s)
            do
            {
                card = DrawCard();
                yield return card;
            } while (card.Rolling);

            // Second card
            yield return DrawCard();
        }

        protected override Card GetResultCard(AttackKind kind, IReadOnlyList<Card> cards)
        {
            return kind switch
            {
                AttackKind.Normal => cards.First(c => !c.Rolling),
                AttackKind.Advantage => cards.SkipWhile(c => c.Rolling).Take(2).MaxBy(c => c.Value) ?? cards.First(),
                AttackKind.Disadvantage => cards.SkipWhile(c => c.Rolling).Take(2).MinBy(c => c.Value) ?? cards.First(),
                _ => throw new NotImplementedException()
            };
        }
    }

    public static class AttackDeckSimulatorFactory
    {
        public static AttackDeckSimulator<TEngine> Create<TEngine>(Game game, IEnumerable<CardGroup> cardValues) where TEngine : BaseEngine
        {
            return game switch
            {
                Game.Gloomhaven => new GloomhavenAttackDeckSimulator<TEngine>(cardValues),
                Game.Frosthaven => new FrosthavenAttackDeckSimulator<TEngine>(cardValues),
                _ => throw new NotImplementedException()
            };
        }
    }

    public record AttackResult(Card Normal, Card Advantage, Card Disadvantage);

    public enum Game
    {
        Gloomhaven,
        Frosthaven
    }

    public enum AttackKind
    {
        Normal,
        Advantage,
        Disadvantage
    }
}
