using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CoCCGen.Core;

public static class COCSkillHelper {
    public static COCSkill[] Load(string filename) {
        try {
            string jsonString = File.ReadAllText(filename);
            COCSkill[]? skills = JsonSerializer.Deserialize<COCSkill[]>(jsonString);
            return skills ?? Array.Empty<COCSkill>();
        }
        catch (Exception ex) {
            Console.WriteLine($"Error reading the file: {ex.Message}");
            return Array.Empty<COCSkill>();
        }
    }
}
