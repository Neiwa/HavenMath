// See https://aka.ms/new-console-template for more information
using HavenAttackModMath;
using System.Diagnostics;

var serializerOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

for (int iArg = 0; iArg < args.Length; iArg++)
{
    var jsonString = File.ReadAllText(args[iArg]);

    //var jsonString = "[{'tag': '0', 'value': 0, 'rolling': true, 'count': 6}, {'tag': '+1', 'value': 10, 'rolling': false, 'count': 5}, {'tag': '-1', 'value': -10, 'rolling': false, 'count': 5}]".Replace("'", "\"");

    var cardGroups = System.Text.Json.JsonSerializer.Deserialize<List<CardGroup>>(jsonString, serializerOptions) ?? throw new ArgumentException();

    var cards = cardGroups.Select(Card.FromCardGroup).ToList();

    var variants = Enum.GetValues<Game>().SelectMany(g => Enum.GetValues<AttackKind>().Select(k => new Variant(g, k))).ToList();
    var variantsResults = variants.ToDictionary(v => v, _ => cards.ToDictionary(key => key, _ => 0));

    const int iterations = 100_000;

    var sw = Stopwatch.StartNew();

    foreach (var variant in variants)
    {
        var sim = AttackDeckSimulatorFactory.Create<EngineV2>(variant.Game, cardGroups);
        var resDict = variantsResults[variant];
        for (int i = 0; i < iterations; i++)
        {
            var result = sim.Attack(variant.AttackKind);
            resDict[result]++;
        }
    }

    sw.Stop();

    Console.WriteLine($"{args[iArg],-40} {iterations * variants.Count:# ### ### ### ###} attacks in {sw.Elapsed:s\\.fff} sec");

    int getNegative(Dictionary<Card, int> dict)
    {
        return cards.Where(c => c.Value < 0).Sum(c => dict[c]);
    }

    int getZeroOrPositive(Dictionary<Card, int> dict)
    {
        return cards.Where(c => c.Value >= 0).Sum(c => dict[c]);
    }

    int getPositive(Dictionary<Card, int> dict)
    {
        return cards.Where(c => c.Value > 0).Sum(c => dict[c]);
    }

    int getMiss(Dictionary<Card, int> dict)
    {
        return cards.Where(c => c.Value == -100).Sum(c => dict[c]);
    }

    void displayRow(string what, Func<Variant, string> fn)
    {
        const int valueLength = 6;
        var valueString = string.Join(" || ", Enum.GetValues<Game>().Select(g => string.Join(" | ", Enum.GetValues<AttackKind>().Select(k => fn(new Variant(g, k))[..valueLength]))));

        Console.WriteLine($" {what,5} | {valueString,-valueLength} |");
    }
    void writeRow(string what, Func<Dictionary<Card, int>, int> fn)
    {
        displayRow(what, v => $"{fn(variantsResults[v]) / (double)iterations,6:P0}");
    }

    Console.WriteLine("       |        Gloomhaven        ||        Frosthaven        |");
    displayRow("Card", v => v.AttackKind.ToString());
    foreach (var card in cards)
    {
        writeRow(card.ToString(), d => d[card]);
    }
    Console.WriteLine();

    writeRow("Pos", getPositive);
    writeRow(">=0", getZeroOrPositive);
    writeRow("Neg", getNegative);
    writeRow("Miss", getMiss);
    Console.WriteLine();
}

public record Variant(Game Game, AttackKind AttackKind);