namespace HavenAttackModMath
{
    public record class Card(string Tag, int Value, bool Rolling, bool Terminal)
    {
        public override string ToString()
        {
            return $"{(Rolling ? "^" : "")}{Tag}";
        }
    }

    public record class CardGroup(string Tag, int Value, bool Rolling, bool Terminal, int Count);

}