using System;
using System.Collections.Generic;
using System.Linq;

class CoffeeShopCFG
{
    // CFG Terminals
    static HashSet<string> Customers = new HashSet<string> { "Alice", "Bob", "Carla", "David" };
    static HashSet<string> Drinks = new HashSet<string> { "Coffee", "Latte", "Espresso", "Tea" };
    static HashSet<string> Foods = new HashSet<string> { "Sandwich", "Muffin", "Bagel" };
    static HashSet<string> Modifiers = new HashSet<string> { "with Milk", "with Sugar", "extra Shot" };

    public class Token
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public Token(string v, string t)
        {
            Value = v;
            Type = t;
        }
    }

    static void Main()
    {
        // Test Inputs (at least 5)
        string[] tests = {
            "Alice orders Coffee",
            "Bob orders Latte, Sandwich with Sugar",
            "Carla orders Espresso, Bagel",
            "David orders Tea, Muffin with Milk",
            "Alice orders Coffee, Muffin, Sandwich extra Shot"
        };

        foreach (string input in tests)
        {
            Console.WriteLine($"\n=== Input: {input} ===");
            RunTokenizer(input);
        }
    }

    static void RunTokenizer(string input)
    {
        // Phase 1: Tokenization
        List<Token> tokens = Tokenize(input);

        Console.WriteLine("\nPhase 1: CFG-based classification");

        // Align columns for clean output
        int maxLen = tokens.Max(t => t.Value.Length);
        foreach (var t in tokens)
            Console.WriteLine($"{t.Value.PadRight(maxLen)} → {t.Type}");

        Console.WriteLine("\nPhase 2: Derivation (leftmost expansion)");
        Derive(tokens);
    }

    static List<Token> Tokenize(string input)
    {
        List<Token> tokens = new List<Token>();

        // Split by comma (order separator)
        string[] parts = input.Split(',');

        foreach (string part in parts)
        {
            string segment = part.Trim();
            string[] words = segment.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];

                if (Customers.Contains(w))
                    tokens.Add(new Token(w, "<customer>"));

                else if (w == "orders")
                    tokens.Add(new Token(w, "\"orders\""));

                else if (Drinks.Contains(w))
                    tokens.Add(new Token(w, "<drink>"));

                else if (Foods.Contains(w))
                    tokens.Add(new Token(w, "<food>"));

                else if (w == "with" && i + 1 < words.Length)
                {
                    string mod = w + " " + words[i + 1];
                    if (Modifiers.Contains(mod))
                    {
                        tokens.Add(new Token(mod, "<modifier>"));
                        i++;
                    }
                }
                else if (w == "extra" && i + 1 < words.Length && words[i + 1] == "Shot")
                {
                    tokens.Add(new Token("extra Shot", "<modifier>"));
                    i++;
                }
            }

            if (!segment.Equals(parts.Last()))
                tokens.Add(new Token(",", "<delimiter>"));
        }
        return tokens;
    }

    static void Derive(List<Token> tokens)
    {
        Console.WriteLine("<sentence>");
        Console.WriteLine("→ <customer> \"orders\" <order>");
        Console.WriteLine($"→ {tokens[0].Value} \"orders\" <order>");

        int idx = 2; // after customer + "orders"
        ExpandOrder(tokens.Skip(idx).ToList(), $"{tokens[0].Value} orders");
    }

    static void ExpandOrder(List<Token> orderTokens, string prefix)
    {
        if (orderTokens.Count == 0) return;

        if (orderTokens[0].Type == "<drink>")
        {
            Console.WriteLine($"→ {prefix} <drink>");
            Console.WriteLine($"→ {prefix} {orderTokens[0].Value}");
        }
        else if (orderTokens[0].Type == "<food>")
        {
            Console.WriteLine($"→ {prefix} <food>");
            Console.WriteLine($"→ {prefix} {orderTokens[0].Value}");

            if (orderTokens.Count > 1 && orderTokens[1].Type == "<modifier>")
            {
                Console.WriteLine($"→ {prefix} {orderTokens[0].Value} <modifier>");
                Console.WriteLine($"→ {prefix} {orderTokens[0].Value} {orderTokens[1].Value}");
            }
        }

        // Handle multiple items separated by commas
        int commaIndex = orderTokens.FindIndex(t => t.Type == "<delimiter>");
        if (commaIndex != -1 && commaIndex + 1 < orderTokens.Count)
        {
            Console.WriteLine($"→ {prefix} {orderTokens[0].Value} , <order>");
            ExpandOrder(orderTokens.Skip(commaIndex + 1).ToList(), $"{prefix} {orderTokens[0].Value},");
        }
    }
}
