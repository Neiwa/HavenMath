namespace HavenAttackModMath
{
    public record class Card(string Tag, int Value, bool Rolling, bool Terminal)
    {
        public override string ToString()
        {
            return $"{(Rolling ? "^" : "")}{Tag}";
        }

        public static Card FromCardGroup(CardGroup cardGroup)
        {
            return new Card(cardGroup.Tag, cardGroup.Value, cardGroup.Rolling, cardGroup.Terminal);
        }
    }

    public record class CardGroup(string Tag, int Value, bool Rolling, bool Terminal, int Count);

}