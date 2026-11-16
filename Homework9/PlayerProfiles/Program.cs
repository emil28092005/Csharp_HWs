using System.Globalization;
using System.Text.Json;

class Program
{
    static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "actions.log");
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    static void Main()
    {
        while (true)
        {
            Console.WriteLine("\n=== Управление профилями ===");
            Console.WriteLine("1. Создать профиль");
            Console.WriteLine("2. Записать результат игры");
            Console.WriteLine("3. Показать профиль");
            Console.WriteLine("4. Показать лог действий");
            Console.WriteLine("5. Выход");
            Console.Write("Выберите пункт: ");

            try
            {
                switch (Console.ReadLine())
                {
                    case "1":
                        CreateProfile();
                        break;
                    case "2":
                        RecordResult();
                        break;
                    case "3":
                        ShowProfile();
                        break;
                    case "4":
                        if (File.Exists(LogPath))
                            Console.Write(File.ReadAllText(LogPath));
                        else
                            Console.WriteLine("Лог пока пуст.");
                        break;
                    case "5":
                    case null:
                        return;
                    default:
                        Console.WriteLine("Нет такого пункта меню.");
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    static string ReadNickname()
    {
        Console.Write("Никнейм: ");
        string nickname = (Console.ReadLine() ?? throw new EndOfStreamException()).Trim();
        if (nickname.Length == 0)
            throw new ArgumentException("Никнейм не должен быть пустым.");

        foreach (char symbol in nickname)
        {
            if (!char.IsLetterOrDigit(symbol) && symbol != '_' && symbol != '-')
                throw new ArgumentException("В никнейме допустимы буквы, цифры, '_' и '-'.");
        }
        return nickname;
    }

    static string ProfilePath(string nickname)
    {
        return Path.Combine(AppContext.BaseDirectory, $"profile_{nickname}.json");
    }

    static void CreateProfile()
    {
        string nickname = ReadNickname();
        string path = ProfilePath(nickname);
        if (File.Exists(path))
        {
            Console.WriteLine("Профиль уже существует!");
            return;
        }

        PlayerProfile profile = new PlayerProfile { Nickname = nickname };
        SaveProfile(profile);
        AppendLog($"Создан профиль: {nickname}");
        Console.WriteLine("Профиль создан.");
    }

    static PlayerProfile? LoadProfile(string nickname)
    {
        string path = ProfilePath(nickname);
        if (!File.Exists(path))
        {
            Console.WriteLine("Профиль не найден");
            return null;
        }

        PlayerProfile profile = JsonSerializer.Deserialize<PlayerProfile>(File.ReadAllText(path))
            ?? throw new JsonException("В файле нет профиля.");
        if (profile.Nickname != nickname || profile.GamesPlayed < 0 ||
            profile.Wins < 0 || profile.Wins > profile.GamesPlayed)
            throw new JsonException("Неверные данные профиля.");
        return profile;
    }

    static void RecordResult()
    {
        string nickname = ReadNickname();
        PlayerProfile? profile = LoadProfile(nickname);
        if (profile == null)
            return;

        Console.Write("Результат (win/loss): ");
        string result = (Console.ReadLine() ?? throw new EndOfStreamException()).Trim().ToLowerInvariant();
        if (result != "win" && result != "loss")
        {
            Console.WriteLine("Введите win или loss.");
            return;
        }

        profile.GamesPlayed++;
        if (result == "win")
            profile.Wins++;
        profile.WinRate = profile.Wins * 100.0 / profile.GamesPlayed;
        SaveProfile(profile);

        string rate = profile.WinRate.ToString("F1", CultureInfo.InvariantCulture);
        AppendLog($"{nickname}: {result} (Всего игр: {profile.GamesPlayed}, Побед: {profile.Wins}, WinRate: {rate}%)");
        Console.WriteLine("Результат сохранён.");
    }

    static void ShowProfile()
    {
        PlayerProfile? profile = LoadProfile(ReadNickname());
        if (profile == null)
            return;

        Console.WriteLine($"Никнейм: {profile.Nickname}");
        Console.WriteLine($"Всего игр: {profile.GamesPlayed}");
        Console.WriteLine($"Побед: {profile.Wins}");
        Console.WriteLine($"WinRate: {profile.WinRate.ToString("F1", CultureInfo.InvariantCulture)}%");
    }

    static void SaveProfile(PlayerProfile profile)
    {
        File.WriteAllText(ProfilePath(profile.Nickname), JsonSerializer.Serialize(profile, JsonOptions));
    }

    static void AppendLog(string message)
    {
        string time = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        File.AppendAllText(LogPath, $"[{time}] {message}{Environment.NewLine}");
    }
}
