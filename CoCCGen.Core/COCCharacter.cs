using static System.Net.Mime.MediaTypeNames;

namespace CoCCGen.Core;

public class COCCharacter {
    public required string Name { get; init; }
    public required string Age { get; init; }
    public required string Occupation { get; init; }
    public required string Backstory { get; init; }
    public required string Traits { get; init; }

    public COCCharacterAttributes Attributes { get; } = new COCCharacterAttributes();

    public COCCharacterScores Scores { get; } = new COCCharacterScores();

    public List<COCCharacterSkill> Skills { get; } = new List<COCCharacterSkill>();

    public required string OccupationalSkills { get; init; }
    public required string NonOccupationalSkills { get; init; }

    internal COCCharacterSkill GetSkill(COCSkill skillData, string? specialization) {
        var sk = Skills.FirstOrDefault(s => s.SkillName == skillData.Name && s.Specialization == specialization);
        if (sk == null) {
            sk = new COCCharacterSkill() {
                SkillName = skillData.Name,
                InitialValue = GetDefaultValue(skillData.DefaultValue),
                Bonuses = 0,
                Specialization = skillData.Specialization ? specialization : null,
            };
            Skills.Add(sk);
        }
        return sk;

        int GetDefaultValue(int defaultValue) {
            return defaultValue switch {
                -20 => Attributes.Dexterity / 2,
                -50 => Attributes.Education,
                _ => defaultValue
            };
        }
    }
}

public class COCCharacterScores {
    public int HP { get; set; }
    public int MP { get; set; }
    public int Build { get; set; }
    public int Move { get; set; }
    public int Luck { get; set; }
    public int DamageBonusValue { get; set; }

    public string DamageBonus => DamageBonusValue switch {
        <= 0 => $"{DamageBonusValue}",
        _ => $"+{DamageBonusValue / 10}D{DamageBonusValue % 10}"
    };
}

public class COCCharacterAttributes {
    public int[] Values { get; set; } = new int[8];
    public int Strength { get { return Values[(int)Attributes.STR]; } set { Values[(int)Attributes.STR] = value; } }
    public int Constitution { get { return Values[(int)Attributes.CON]; } set { Values[(int)Attributes.CON] = value; } }
    public int Size { get { return Values[(int)Attributes.SIZ]; } set { Values[(int)Attributes.SIZ] = value; } }
    public int Dexterity { get { return Values[(int)Attributes.DEX]; } set { Values[(int)Attributes.DEX] = value; } }
    public int Intelligence { get { return Values[(int)Attributes.INT]; } set { Values[(int)Attributes.INT] = value; } }
    public int Appearance { get { return Values[(int)Attributes.APP]; } set { Values[(int)Attributes.APP] = value; } }
    public int Power { get { return Values[(int)Attributes.POW]; } set { Values[(int)Attributes.POW] = value; } }
    public int Education { get { return Values[(int)Attributes.EDU]; } set { Values[(int)Attributes.EDU] = value; } }

    public override string ToString() {
        return $"STR: {Strength}, CON: {Constitution}, SIZ: {Size}, DEX: {Dexterity}, INT: {Intelligence}, APP: {Appearance}, POW: {Power}, EDU: {Education}";
    }
}

public class COCCharacterSkill : IComparable {
    public required string SkillName { get; init; }

    public string? Specialization { get; init; }

    public bool HasSpecialization => Specialization != null;

    public required int InitialValue { get; init; }

    public required int Bonuses { get; set; }

    public int Value => InitialValue + Bonuses;

    public override string ToString() {
        var name = Specialization is not null ? $"{SkillName} ({Specialization})" : SkillName;
        var val = $"{Value}% ({Value / 2}/{Value / 5})";

        return $"{name}: {val}";
    }

    public override bool Equals(object? obj) {
        if (obj is not null && obj is COCCharacterSkill skill) {
            return skill.SkillName == SkillName && skill.Specialization == Specialization;
        }
        return false;
    }

    public override int GetHashCode() {
        if (Specialization is not null) {
            return SkillName.GetHashCode() ^ Specialization.GetHashCode();
        }

        return SkillName.GetHashCode();
    }

    public int CompareTo(object? obj) {
        if (obj is not null && obj is COCCharacterSkill skill) {
            var c = skill.SkillName.CompareTo(SkillName);
            if (c == 0 && string.IsNullOrEmpty(Specialization) == false && string.IsNullOrEmpty(skill.Specialization) == false) {
                return skill.Specialization.CompareTo(Specialization);
            }
            else {
                return c;
            }
        }
        return 0;
    }
}
public enum Attributes : int {
    STR = 0,
    CON = 1,
    SIZ = 2,
    DEX = 3,
    INT = 4,
    APP = 5,
    POW = 6,
    EDU = 7,
}