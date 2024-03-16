namespace HavenAttackModMath
{
    public abstract class BaseEngine(IReadOnlyList<Card> cards)
    {
        protected readonly IReadOnlyList<Card> Cards = cards;
        protected readonly Random Rng = new();

        public abstract Card DrawCard();

        public abstract void Shuffle();
    }

    public class EngineV1 : BaseEngine
    {
        private readonly HashSet<int> _drawn;

        public EngineV1(IReadOnlyList<Card> cards) : base(cards)
        {
            _drawn = new HashSet<int>(Cards.Count);
        }

        public override Card DrawCard()
        {
            if (_drawn.Count == Cards.Count)
            {
                Shuffle();
            }

            int r;
            do
            {
                r = Rng.Next(0, Cards.Count);
            } while (_drawn.Contains(r));

            _drawn.Add(r);

            return Cards[r];
        }

        public override void Shuffle()
        {
            _drawn.Clear();
        }
    }

    public class EngineV2 : BaseEngine
    {
        private readonly int[] _deck;
        private int _current;

        public EngineV2(IReadOnlyList<Card> cards) : base(cards)
        {
            _deck = Enumerable.Range(0, Cards.Count).ToArray();
            Shuffle();
        }

        public override Card DrawCard()
        {
            var card = _deck[_current++];
            if (_current > _deck.Length)
            {
                Shuffle();
            }

            return Cards[card];
        }

        public override void Shuffle()
        {
            int n = _deck.Length;
            while (n > 1)
            {
                int k = Rng.Next(n--);
                (_deck[k], _deck[n]) = (_deck[n], _deck[k]);
            }

            _current = 0;
        }
    }
}
