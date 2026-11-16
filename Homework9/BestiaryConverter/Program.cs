using System.Text.Json;

class Program
{
    static void Main()
    {
        string csvPath = Path.Combine(AppContext.BaseDirectory, "monsters.csv");
        string jsonPath = Path.Combine(AppContext.BaseDirectory, "monsters.json");

        try
        {
            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length == 0 || lines[0].Trim() != "Name;Health;Damage;Type")
            {
                Console.WriteLine("Ошибка: ожидается заголовок Name;Health;Damage;Type.");
                return;
            }

            List<Monster> monsters = new List<Monster>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                    continue;

                string[] parts = lines[i].Split(';');
                if (parts.Length != 4 || string.IsNullOrWhiteSpace(parts[0]) ||
                    string.IsNullOrWhiteSpace(parts[3]) ||
                    !int.TryParse(parts[1], out int health) || health < 0 ||
                    !int.TryParse(parts[2], out int damage) || damage < 0)
                {
                    Console.WriteLine($"Ошибка в строке {i + 1}: проверьте данные монстра.");
                    return;
                }

                monsters.Add(new Monster
                {
                    Name = parts[0].Trim(),
                    Health = health,
                    Damage = damage,
                    Type = parts[3].Trim()
                });
            }

            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(monsters, options));

            string json = File.ReadAllText(jsonPath);
            List<Monster> loaded = JsonSerializer.Deserialize<List<Monster>>(json)
                ?? throw new JsonException("В JSON нет списка монстров.");
            foreach (Monster monster in loaded)
                Console.WriteLine($"[{monster.Type}] {monster.Name} — HP: {monster.Health}, DMG: {monster.Damage}");

            Console.WriteLine($"\nОбщее количество монстров: {loaded.Count}");
            if (loaded.Count == 0)
                return;

            Monster healthiest = loaded[0];
            Monster strongest = loaded[0];
            foreach (Monster monster in loaded)
            {
                if (monster.Health > healthiest.Health)
                    healthiest = monster;
                if (monster.Damage > strongest.Damage)
                    strongest = monster;
            }

            Console.WriteLine($"Наибольшее здоровье: {healthiest.Name} (HP: {healthiest.Health})");
            Console.WriteLine($"Наибольший урон: {strongest.Name} (DMG: {strongest.Damage})");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Console.WriteLine($"Ошибка конвертации: {ex.Message}");
        }
    }
}
