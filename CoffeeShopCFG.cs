using System;
using System.Collections.Generic;
using System.Linq;

class CoffeeShopCfg
{
    // === CFG Terminals ===
    static HashSet<string> titleSet = new HashSet<string> { "Mr.", "Ms." };
    static HashSet<string> nameSet = new HashSet<string> { "Alice", "Bob", "Carla", "David", "Favio", "Miggy" };
    static HashSet<string> quantitySet = new HashSet<string> { "One", "Two", "Three", "Four", "Five" };
    static HashSet<string> foodSet = new HashSet<string> { "sandwich", "salad", "cake", "muffin", "pasta" };
    static HashSet<string> drinkSet = new HashSet<string> { "coffee", "tea", "latte", "smoothie", "juice" };
    static HashSet<string> modifierSet = new HashSet<string> { "with Milk", "with Sugar", "extra Shot" };
    static HashSet<string> paymentSet = new HashSet<string> { "Cash", "Credit Card", "Debit Card", "Paypal" };

    public class Token
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public Token(string value, string type)
        {
            Value = value;
            Type = type;
        }
    }

    static void Main()
    {
        Console.WriteLine("=== Coffee Shop Grammar (CFG) ===\n");

        Console.WriteLine("<sentence> → <customer> \"orders\" <order> \"and pays using\" <payment>");
        Console.WriteLine("<customer> → <name> | <title> <name>");
        Console.WriteLine("<title> → " + string.Join(" | ", titleSet));
        Console.WriteLine("<name> → " + string.Join(" | ", nameSet));
        Console.WriteLine("<order> → <item> | <item> \",\" <order>");
        Console.WriteLine("<quantity> → " + string.Join(" | ", quantitySet));
        Console.WriteLine("<item> → [<quantity>] <food> [<modifier>] | [<quantity>] <drink> [<modifier>]");
        Console.WriteLine("<food> → " + string.Join(" | ", foodSet));
        Console.WriteLine("<drink> → " + string.Join(" | ", drinkSet));
        Console.WriteLine("<modifier> → " + string.Join(" | ", modifierSet));
        Console.WriteLine("<payment> → " + string.Join(" | ", paymentSet));
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("Enter an order (or type EXIT to quit):");
            string input = Console.ReadLine();

            if (input.Trim().ToUpper() == "EXIT")
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            Console.WriteLine($"\n=== Input: {input} ===");
            List<Token> tokenList = Tokenize(input);

            if (tokenList.Count == 0)
            {
                Console.WriteLine("Error: No valid tokens found. Please try again.\n");
                continue;
            }

            if (!Validate(tokenList))
            {
                Console.WriteLine("Error: Input does not match the grammar. Please try again.\n");
                continue;
            }

            Console.WriteLine("\nPhase 1: CFG-based classification");
            foreach (var token in tokenList)
                Console.WriteLine($"{token.Value} → {token.Type}");

            Console.WriteLine("\nPhase 2: Derivation (leftmost derivation)");
            Derive(tokenList);

            Console.WriteLine("\nInput accepted!\n");
        }
    }

    static List<Token> Tokenize(string input)
    {
        List<Token> tokenList = new List<Token>();
        string[] parts = input.Split(' ');

        for (int i = 0; i < parts.Length; i++)
        {
            string word = parts[i];

            if (titleSet.Contains(word))
                tokenList.Add(new Token(word, "<title>"));
            else if (nameSet.Contains(word))
                tokenList.Add(new Token(word, "<name>"));
            else if (word == "orders")
                tokenList.Add(new Token(word, "\"orders\""));
            else if (quantitySet.Contains(word))
                tokenList.Add(new Token(word, "<quantity>"));
            else if (foodSet.Contains(word))
                tokenList.Add(new Token(word, "<food>"));
            else if (drinkSet.Contains(word))
                tokenList.Add(new Token(word, "<drink>"));
            else if (word == "with" && i + 1 < parts.Length)
            {
                string mod = word + " " + parts[i + 1];
                if (modifierSet.Contains(mod))
                {
                    tokenList.Add(new Token(mod, "<modifier>"));
                    i++;
                }
            }
            else if (word == "extra" && i + 1 < parts.Length && parts[i + 1] == "Shot")
            {
                tokenList.Add(new Token("extra Shot", "<modifier>"));
                i++;
            }
            else if (word == "and" && i + 2 < parts.Length && parts[i + 1] == "pays" && parts[i + 2] == "using")
            {
                tokenList.Add(new Token("and pays using", "\"and pays using\""));
                i += 2;
            }
            else if (paymentSet.Contains(word + " " + (i + 1 < parts.Length ? parts[i + 1] : "")))
            {
                string pay = word + " " + parts[i + 1];
                tokenList.Add(new Token(pay, "<payment>"));
                i++;
            }
            else if (paymentSet.Contains(word))
                tokenList.Add(new Token(word, "<payment>"));
            else if (word == ",")
                tokenList.Add(new Token(word, "<delimiter>"));
        }
        return tokenList;
    }

    static bool Validate(List<Token> tokenList)
    {
        if (tokenList.Count < 5) return false;

        int index = 0;
        if (tokenList[index].Type == "<title>")
        {
            if (index + 1 >= tokenList.Count || tokenList[index + 1].Type != "<name>")
                return false;
            index += 2;
        }
        else if (tokenList[index].Type == "<name>")
        {
            index++;
        }
        else
        {
            return false;
        }

        if (index >= tokenList.Count || tokenList[index].Value != "orders") return false;
        if (!tokenList.Any(t => t.Value == "and pays using")) return false;
        if (!tokenList.Any(t => t.Type == "<payment>")) return false;

        return true;
    }

    static void Derive(List<Token> tokenList)
    {
        Console.WriteLine("<sentence>");
        Console.WriteLine("⇒ <customer> \"orders\" <order> \"and pays using\" <payment>");

        int index = 0;
        if (tokenList[index].Type == "<title>")
        {
            Console.WriteLine("⇒ <title> <name> \"orders\" <order> \"and pays using\" <payment>");
            Console.WriteLine($"⇒ {tokenList[index].Value} {tokenList[index + 1].Value} \"orders\" <order> \"and pays using\" <payment>");
            index += 2;
        }
        else
        {
            Console.WriteLine("⇒ <name> \"orders\" <order> \"and pays using\" <payment>");
            Console.WriteLine($"⇒ {tokenList[index].Value} \"orders\" <order> \"and pays using\" <payment>");
            index++;
        }

        Console.WriteLine($"⇒ {string.Join(" ", tokenList.Select(t => t.Value))}");
    }
}
