using System.Text.RegularExpressions;
using CoCCGen.Core;

namespace CoCCGen.CLI;

internal class Program {

    static OpenAIHelper ai;

    static async Task Main(string[] args) {
        Console.WriteLine("Call of Cthulhu Character Generator with AI");
        Console.WriteLine("Using GPT-4o");
        Console.WriteLine("Proof of concept demo");
        Console.WriteLine();

        if (File.Exists("apikey.txt") == false) {
            Console.WriteLine("Please create a file called 'apikey.txt' with your OpenAI API key in it.");
            return;
        }

        ai = new OpenAIHelper(File.ReadAllText("apikey.txt"));

        var country = "Great Britain";
        var lang = "English";
        var time = "present time";

        //Console.WriteLine("Please give a short request for a character: ");
        //var shortRequest = Console.ReadLine();
        // choose a random character idea
        var shortRequest = Characters.Ideas.Choose();
        //var shortRequest = "A professor of languages at the University of Helsinki, studying occult history and a skilled brawler. Aged 63.";

        var generator = new AICharacterGenerator(ai);
        var character = await generator.GenerateCharacter(shortRequest, country, time, lang);

        // print to screen
        Console.WriteLine("=======================================");
        Console.WriteLine($"{character.Name}, Age: {character.Age}, Occupation: {character.Occupation}");
        Console.WriteLine(character.Attributes.ToString());
        Console.WriteLine($"HP: {character.Scores.HP}, DB: {character.Scores.DamageBonus}, Build: {character.Scores.Build}, Move: {character.Scores.Move}, MP: {character.Scores.MP}, Luck: {character.Scores.Luck}");
        Console.WriteLine("--");
        foreach (var sk in character.Skills.OrderBy(sk => sk.SkillName).ThenBy(sk => sk.Specialization)) {
            Console.WriteLine(sk.ToString());
        }
        Console.WriteLine("--");
        Console.WriteLine("Backstory:");
        Console.WriteLine();
        Console.WriteLine(character.Backstory);
        Console.WriteLine();
        Console.WriteLine(character.Traits);

        //debug
        Console.WriteLine("--");
        Console.WriteLine("Debug");
        Console.WriteLine(shortRequest);
        Console.WriteLine(character.OccupationalSkills);
        Console.WriteLine(character.NonOccupationalSkills);
        Console.WriteLine("--");
        Console.WriteLine("AI token usage");
        Console.WriteLine($" > input tokens: {ai.InputTokens}");
        Console.WriteLine($" > output tokens: {ai.OutputTokens}");
        Console.WriteLine($" > total tokens: {ai.TotalTokens}");

        var roughFilename = $"{character.Name} {character.Age} {character.Occupation}.txt";
        var validFilename = Regex.Replace(roughFilename, @"[^a-zA-Z0-9\.]", "_");
        // now print the same things as the screen into the text file
        using (var file = new StreamWriter(validFilename)) {
            file.WriteLine($"{character.Name}, Age: {character.Age}, Occupation: {character.Occupation}");
            file.WriteLine(character.Attributes.ToString());
            file.WriteLine($"HP: {character.Scores.HP}, DB: {character.Scores.DamageBonus}, Build: {character.Scores.Build}, Move: {character.Scores.Move}, MP: {character.Scores.MP}, Luck: {character.Scores.Luck}");
            file.WriteLine("--");
            foreach (var sk in character.Skills.OrderBy(sk => sk.SkillName).ThenBy(sk => sk.Specialization)) {
                file.WriteLine(sk.ToString());
            }
            file.WriteLine("--");
            file.WriteLine("Backstory:");
            file.WriteLine();
            file.WriteLine(character.Backstory);
            file.WriteLine();
            file.WriteLine(character.Traits);

            file.WriteLine("--");
            file.WriteLine("Debug");
            file.WriteLine(shortRequest);
            file.WriteLine(character.OccupationalSkills);
            file.WriteLine(character.NonOccupationalSkills);

        }
    }




}
