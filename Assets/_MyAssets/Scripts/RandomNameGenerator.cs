using UnityEngine;

public class RandomNameGenerator
{
    public static string[] adverbs = { "Offensively", "Dimly", "Surprisingly", "Painfully", "Curiously", "Coyly", "Viciously", "Merrily", "Bleakly", "Boldly", "Deftly", "Elegantly", "Lazily", "Only", "Upbeat", "Seldom"};
    public static string[] adjectives = { "Swift", "Shiny", "Acidic", "Mammoth", "Thin", "Alive", "Tearful", "Spiteful", "Lively", "Cuddly", "Sassy", "Classy", "Torpid", "Hilarious", "Simplistic", "Fanatical", "Ruthless"};
    public static string[] nouns = { "Snake", "Shade", "Queen", "Machine", "Wretch", "Cat", "Frog", "Dog", "Lion", "Boss", "Champ", "Egg", "Coach", "Fish", "Lizard", "Wizard", "Hero", "Villian", "Optimist", "Pessimist"};

    public static string GetRandomName()
    {
        return adverbs[Random.Range(0, adverbs.Length)] + " " + adjectives[Random.Range(0, adjectives.Length)] + " " + nouns[Random.Range(0, nouns.Length)];
    }

    public static string GetRandomName(int length)
    {
        switch (length)
        {
            case 0:
                return nouns[Random.Range(0, nouns.Length)];

            case 1:
                return adjectives[Random.Range(0, adjectives.Length)] + " " + nouns[Random.Range(0, nouns.Length)];

            default:
                return adverbs[Random.Range(0, adverbs.Length)] + " " + adjectives[Random.Range(0, adjectives.Length)] + " " + nouns[Random.Range(0, nouns.Length)];
        }
    }
}