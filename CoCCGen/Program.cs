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
        using ( var sw = new StreamWriter(Console.OpenStandardOutput()) ) {
            Output(character, sw, shortRequest, true);
        }

        // now print the same things as the screen into the text file
        var roughFilename = $"{character.Name} {character.Age} {character.Occupation}.txt";
        var validFilename = Regex.Replace(roughFilename, @"[^a-zA-Z0-9\.]", "_");
        using (var file = new StreamWriter(validFilename)) {
            Output(character, file, shortRequest);
        }
    }

    private static void Output(COCCharacter character, StreamWriter output, string shortRequest, bool debug = false) {
        output.WriteLine($"{character.Name}, Age: {character.Age}, Occupation: {character.Occupation}");
        output.WriteLine(character.Attributes.ToString());
        output.WriteLine($"HP: {character.Scores.HP}, DB: {character.Scores.DamageBonus}, Build: {character.Scores.Build}, Move: {character.Scores.Move}, MP: {character.Scores.MP}, Luck: {character.Scores.Luck}");
        output.WriteLine("--");
        foreach (var sk in character.Skills.OrderBy(sk => sk.SkillName).ThenBy(sk => sk.Specialization)) {
            output.WriteLine(sk.ToString());
        }
        output.WriteLine("--");
        output.WriteLine("Backstory:");
        output.WriteLine();
        output.WriteLine(character.Backstory);
        output.WriteLine();
        output.WriteLine(character.Traits);

        //debug
        output.WriteLine("--");
        output.WriteLine("Debug");
        output.WriteLine(shortRequest);
        output.WriteLine(character.OccupationalSkills);
        output.WriteLine(character.NonOccupationalSkills);
        output.WriteLine("--");
        output.WriteLine("AI token usage");
        output.WriteLine($" > input tokens: {ai.InputTokens}");
        output.WriteLine($" > output tokens: {ai.OutputTokens}");
        output.WriteLine($" > total tokens: {ai.TotalTokens}");
    }
}
