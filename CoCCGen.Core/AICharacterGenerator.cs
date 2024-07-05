using System.Text.RegularExpressions;

namespace CoCCGen.Core;

public class AICharacterGenerator {
    private readonly OpenAIHelper openAI;

    private readonly COCSkill[] allSkills = COCSkillHelper.Load("skills.json");

    public AICharacterGenerator(OpenAIHelper openAI) {
        this.openAI = openAI;
    }

    private IEnumerable<string> AllPossibleSkills() {
        foreach (var skillData in allSkills) {
            if (skillData.Specialization && skillData.PossibleSpecializations != null && skillData.PossibleSpecializations.Length > 0) {
                foreach (var spec in skillData.PossibleSpecializations) {
                    yield return $"{skillData.Name} ({spec})";
                }
            }
            else {
                yield return skillData.Name;
            }
        }
    }

    public async Task<COCCharacter> GenerateCharacter(string characterRequest, string country = "USA", string time = "today", string lang = "Finnish") {

        var fullDescription = await GenerateFullDescription(characterRequest, country, time, lang);

        var attributePriority = await GenerateAttributePriority(fullDescription);

        var traits = await GenerateTraits(fullDescription, lang);

        var (occupational, nonOccupational) = await GenerateSkills(fullDescription, time, country);

        var name = fullDescription.Split('\n')[0].Trim();
        var age = fullDescription.Split('\n')[1].Trim();
        var tagline = fullDescription.Split('\n')[2].Trim();
        var backstory = string.Join('\n', fullDescription.Split('\n').Skip(4));

        var attributesWithValues = AllocateAttributes(attributePriority);

        var (db, build) = CalculateDBandBuild(attributesWithValues);
        var move = CalculateMovement(attributesWithValues);
        var mp = attributesWithValues["POW"] / 5;
        var hp = (attributesWithValues["CON"] + attributesWithValues["SIZ"]) / 10;
        var luck = Dice.Roll(3, 6) * 5;

        COCCharacter pc = new COCCharacter() {
            Name = name,
            Age = age,
            Occupation = tagline,
            Backstory = backstory,
            Traits = traits,
            OccupationalSkills = occupational,
            NonOccupationalSkills = nonOccupational
        };

        pc.Scores.DamageBonusValue = db;
        pc.Scores.Build = build;
        pc.Scores.HP = hp;
        pc.Scores.MP = mp;
        pc.Scores.Move = move;
        pc.Scores.Luck = luck;

        foreach (var a in attributesWithValues) {
            pc.Attributes.Values[attributeOrder[a.Key.ToString()]] = a.Value;
        }

        ParseSkills(pc, occupational, nonOccupational);

        return pc;
    }

    private void ParseSkills(COCCharacter pc, string occupational, string nonOccupational) {

        // split the occupational skills into a list by comma, and trim
        var occupationalSkills = occupational.Split(',').Select(s => s.Trim()).ToList();
        var nonOccupationalSkills = nonOccupational.Split(',').Select(s => s.Trim()).ToList();

        var skillNameRegex = new Regex(@"([\w\s\/]*\w+)\s?\(?([\w\s]+)?\)?");

        int[] values = new int[] { 70, 60, 60, 50, 50, 50, 40, 40, 40 };
        int next = 0;

        foreach (var skill in occupationalSkills) {
            LearnOrAdvance(skill, values[next++], 0);
        }

        LearnOrAdvance("Credit Rating", values[next], 0);

        foreach (var skill in nonOccupationalSkills) {
            LearnOrAdvance(skill, 0, 20);
        }

        void LearnOrAdvance(string skill, int finalValue, int bonus) {
            // check if this skill has a specialization 
            var match = skillNameRegex.Match(skill);
            var skillName = match.Groups[1].Value;
            var specialization = match.Groups[2].Success ? match.Groups[2].Value : null;

            var skillData = FindSkill(skill, skillName);
            if (skillData == null) return;

            if (!skillData.Specialization) {
                specialization = null;
            }

            var sk = pc.GetSkill(skillData, specialization);

            if (finalValue != 0) {
                if (sk.Value >= finalValue) {
                    ; // do nothing
                }
                else if (sk.Bonuses == 0) {
                    sk.Bonuses = finalValue - sk.InitialValue;
                }
                else {
                    sk.Bonuses = finalValue - sk.InitialValue - sk.Bonuses;
                }
            }
            if (bonus != 0) {
                sk.Bonuses += bonus;
            }
        }

        COCSkill? FindSkill(string fullName, string skillName) {
            return allSkills.FirstOrDefault(s => s.Name == fullName) ?? allSkills.FirstOrDefault(s => s.Name == skillName);
        }
    }

    #region Generation

    private async Task<string> GenerateFullDescription(string tagline, string country, string time, string lang) {
        var systemPrompt1 = @$"Generate a character for a Call of Cthulhu RPG game. 
Give me 2 parapgraphs describing the character. The story is set {time} in {country}. 
On the first line return the character name by itself, without other info. 
On the second line, return the characters age. 
On the third line, return a tagline or job or other short description, a few words MAXIMUM.
These lines are followed by a blank line, and then the 2 paragraphs of short description and history. 
Answer in {lang}.";
        var userPrompt1 = $"Character description: {tagline}";
        var character = await openAI.CreateCompletionAsync(systemPrompt1, userPrompt1);

        return character;
    }

    private async Task<string> GenerateAttributePriority(string fullDescription) {
        var systemPrompt2 = @$"I will give you a character description of a Call of Cthulhu RPG character. Based on this description, order the following
attributes of the character in descending order of importance. For example, for someone working physical jobs, STR is probably important. For someone working with 
people, APP might be important. For a professor, INT or EDU is most important. The attributes to order are: Strength (STR), Dexterity (DEX), Constitution (CON), 
Size (SIZ), Intelligence (INT), Education (EDU), Power (POW), Appearane (APP). Return ONLY the comma separated list of attributes in descending order of importance. 
DO NOT return anything else, no descriptions or explanations.";
        return await openAI.CreateCompletionAsync(systemPrompt2, fullDescription);
    }

    private async Task<string> GenerateTraits(string fullDescription, string lang) {
        var systemPrompt3 = @$"I will give you a character description of a Call of Cthulhu RPG character. Based on this description, you will determine some
traits of the character to help the player portray a unique character. List the following traits, one per line:

- Ideology/Beliefs
- Significant People
- Meaningful Locations
- Treasured Possessions
- Trait
- Injuries and Scars (could be none)

In the answer, start the line with the trait type, add a colon (:) and then the description. 

For example:
Traits: optimistic, trusting.
Ideology/Belief: when life hands you an opportunity, seize it with both arms.

Answer in {lang}.
";
        return await openAI.CreateCompletionAsync(systemPrompt3, fullDescription);
    }

    private async Task<(string occupational, string nonOccupational)> GenerateSkills(string fullDescription, string time, string country) {

        var allSkills = string.Join(", ", AllPossibleSkills());

        var systemPrompt4 = $@"I will give you a character description of a Call of Cthulhu RPG character. 
Based on this description, you will determine which 8
Call of Cthulhu RPG skills are appropriate for this character as occupation skills. 
You will return each skill in a comma separated list with the most important one first. 
Return only the list, on a single line, and do not return anything else. Do not explain your answer or give explanations or comments.
For Art/Craft, Science, Pilot, Survival also list the specialisation if applicable. Show this in parantheses after the skill name. For example: Survival (forest). 
For own language use Language (Own). 
For other languages, specify the concrete language. For example: Language (German) or Language (Arabic) or Language (Latin).
Do not include Credit Rating in your response. 
The story is set {time} in {country}.

All possible skills: {allSkills}
";
        var skills = await openAI.CreateCompletionAsync(systemPrompt4, fullDescription);

        var nonOccupationSkillCount = Dice.Random.Next(3, 6);
        var systemPrompt5 = $@"I will give you a character description of a Call of Cthulhu RPG character. 
Based on this description, you will determine which {nonOccupationSkillCount} Call of Cthulhu RPG skills are appropriate for this character as non-occupation skills. 
You will return each skill in a comma separated list. 
These skills represent interests of the character, and not necesarily relate to his or her occupation.
Return only the list, on a single line, and do not return anything else. Do not explain your answer or give explanations or comments.
For Art/Craft, Science, Pilot, Survival also list the specialisation if applicable. Show this in parantheses after the skill name. For example: Survival (forest). 
For own language use Language (Own). 
For other languages, specify the concrete language. For example: Language (German) or Language (Arabic) or Language (Latin).
Do not include Credit Rating in your response. 
The story is set {time} in {country}.

All possible skills: {allSkills}
";
        var nonOccupationSkills = await openAI.CreateCompletionAsync(systemPrompt5, fullDescription);

        return (skills, nonOccupationSkills);
    }

    private static Dictionary<string, int> AllocateAttributes(string attributes) {
        // break up the attributes string by splitting on commas, and trimming
        var attributeList = attributes.Split(',').Select(a => a.Trim()).ToList();

        // now order the values array so the attributes are presented in this order: STR, CON, SIZ, DEX, INT, APP, POW, EDU
        var a = new Dictionary<string, int>();

        for (int i = 0; i < attributeList.Count; i++) {
            var attribute = attributeList[i];
            var value = attributeValues[i];
            a.Add(attribute, value);
        }

        return a;
    }

    private static string ToAttributeString(Dictionary<string, int> attributes) {
        var orderedValues = new string[8];

        foreach (var k in attributes.Keys) {
            orderedValues[attributeOrder[k]] = $"{k}: {attributes[k]}";
        }

        // return the ordered values as a string
        return string.Join(", ", orderedValues);

    }

    static Dictionary<string, int> attributeOrder = new Dictionary<string, int> {
        ["STR"] = 0,
        ["CON"] = 1,
        ["SIZ"] = 2,
        ["DEX"] = 3,
        ["INT"] = 4,
        ["APP"] = 5,
        ["POW"] = 6,
        ["EDU"] = 7
    };
    static int[] attributeValues = new int[] { 80, 70, 60, 60, 50, 50, 50, 40 };

    #endregion

    #region Character calculations (move them to base class)
    private static (int db, int build) CalculateDBandBuild(Dictionary<string, int> attributesWithValues) {
        var str_plus_size = attributesWithValues["STR"] + attributesWithValues["SIZ"];

        return str_plus_size switch {
            < 64 => (-2, -2),
            < 84 => (-1, -1),
            < 124 => (0, 0),
            < 164 => (14, 1),
            < 204 => (16, 2),
            < 284 => (26, 3),
            < 364 => (36, 4),
            < 444 => (46, 5),
            < 524 => (56, 6),
            _ => (66, 7)
        };
    }

    private static int CalculateMovement(Dictionary<string, int> attributesWithValues) {
        //If both DEX and STR are each less than SIZ: MOV 7
        //If either STR or DEX is equal to or greater
        //than SIZ, or if all three are equal: MOV 8
        //If both STR and DEX are each greater than SIZ: MOV 9
        if (attributesWithValues["DEX"] < attributesWithValues["SIZ"]
            && attributesWithValues["STR"] < attributesWithValues["SIZ"]) {
            return 7;
        }
        else if (attributesWithValues["DEX"] > attributesWithValues["SIZ"]
            && attributesWithValues["STR"] > attributesWithValues["SIZ"]) {
            return 9;
        }
        else {
            return 8;
        }
    }

    #endregion
}
