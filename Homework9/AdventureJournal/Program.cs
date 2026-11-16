using System.Text.Json;

class Program
{
    static readonly string JournalPath = Path.Combine(AppContext.BaseDirectory, "journal.json");
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    static void Main()
    {
        try
        {
            Run();
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("Ввод завершён.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Console.WriteLine($"Ошибка работы с журналом: {ex.Message}");
        }
    }

    static void Run()
    {
        AdventureJournal journal;
        if (File.Exists(JournalPath))
        {
            journal = JsonSerializer.Deserialize<AdventureJournal>(File.ReadAllText(JournalPath))
                ?? throw new JsonException("В файле нет журнала.");
            if (journal.Quests == null || string.IsNullOrWhiteSpace(journal.HeroName))
                throw new JsonException("Неверные данные журнала.");
            foreach (QuestEntry quest in journal.Quests)
            {
                if (quest == null || string.IsNullOrWhiteSpace(quest.QuestName) ||
                    string.IsNullOrWhiteSpace(quest.Description) ||
                    (quest.Status != "Active" && quest.Status != "Completed" && quest.Status != "Failed"))
                    throw new JsonException("Неверные данные квеста.");
            }
            Console.WriteLine($"Герой: {journal.HeroName}");
        }
        else
        {
            journal = new AdventureJournal { HeroName = ReadText("Имя героя: ") };
            Save(journal);
        }

        while (true)
        {
            Console.WriteLine("\n=== Журнал приключений ===");
            Console.WriteLine("1. Добавить квест");
            Console.WriteLine("2. Завершить квест");
            Console.WriteLine("3. Провалить квест");
            Console.WriteLine("4. Показать все квесты");
            Console.WriteLine("5. Показать только активные квесты");
            Console.WriteLine("6. Выход");
            Console.Write("Выберите пункт: ");

            switch (Console.ReadLine())
            {
                case "1":
                    QuestEntry quest = new QuestEntry
                    {
                        QuestName = ReadText("Название квеста: "),
                        Description = ReadText("Описание: "),
                        Status = "Active"
                    };
                    journal.Quests.Add(quest);
                    Save(journal);
                    Console.WriteLine("Квест добавлен.");
                    break;
                case "2":
                    ChangeStatus(journal, "Completed");
                    break;
                case "3":
                    ChangeStatus(journal, "Failed");
                    break;
                case "4":
                    ShowQuests(journal.Quests);
                    break;
                case "5":
                    ShowQuests(GetActiveQuests(journal));
                    break;
                case "6":
                case null:
                    return;
                default:
                    Console.WriteLine("Нет такого пункта меню.");
                    break;
            }
        }
    }

    static List<QuestEntry> GetActiveQuests(AdventureJournal journal)
    {
        List<QuestEntry> active = new List<QuestEntry>();
        foreach (QuestEntry quest in journal.Quests)
        {
            if (quest.Status == "Active")
                active.Add(quest);
        }
        return active;
    }

    static void ChangeStatus(AdventureJournal journal, string status)
    {
        List<QuestEntry> active = GetActiveQuests(journal);
        if (active.Count == 0)
        {
            Console.WriteLine("Активных квестов нет.");
            return;
        }

        ShowQuests(active);
        Console.Write("Номер квеста: ");
        if (!int.TryParse(Console.ReadLine(), out int number) || number < 1 || number > active.Count)
        {
            Console.WriteLine("Неверный номер квеста.");
            return;
        }

        active[number - 1].Status = status;
        Save(journal);
        Console.WriteLine("Статус квеста изменён.");
    }

    static void ShowQuests(List<QuestEntry> quests)
    {
        if (quests.Count == 0)
            Console.WriteLine("Квестов нет.");

        for (int i = 0; i < quests.Count; i++)
        {
            QuestEntry quest = quests[i];
            Console.WriteLine($"{i + 1}. [{quest.Status}] {quest.QuestName}");
            Console.WriteLine($"   {quest.Description}");
        }
    }

    static void Save(AdventureJournal journal)
    {
        File.WriteAllText(JournalPath, JsonSerializer.Serialize(journal, JsonOptions));
    }

    static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string text = Console.ReadLine() ?? throw new EndOfStreamException();
            if (!string.IsNullOrWhiteSpace(text))
                return text.Trim();
            Console.WriteLine("Введите непустую строку.");
        }
    }
}
