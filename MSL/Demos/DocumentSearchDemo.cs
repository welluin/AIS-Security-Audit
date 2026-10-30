using MSL.Data;

namespace MSL.Demos
{
    public static class DocumentSearchDemo
    {
        public static void Run()
        {
            if (UserSession.CurrentEmployee == null)
            {
                Console.WriteLine("\n[ОТКАЗ] Сначала необходимо авторизоваться.");
                return;
            }

            int clearanceLevel = UserSession.CurrentEmployee.ClearanceLevel;

            Console.Write("\nПоисковый запрос по материалам: ");
            string? term = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(term))
            {
                Console.WriteLine("\nВведите поисковый запрос.");
                return;
            }

            using var connection = DatabaseContext.CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT id, title, secret_level, content
                FROM ClassifiedDocuments
                WHERE secret_level <= @level
                AND title LIKE @term;
            ";

            command.Parameters.AddWithValue("@level", clearanceLevel);
            command.Parameters.AddWithValue("@term", $"%{term}%");

            try
            {
                using var reader = command.ExecuteReader();

                Console.WriteLine("\n--- РЕЗУЛЬТАТЫ ПОИСКА ---");

                bool found = false;

                while (reader.Read())
                {
                    found = true;

                    Console.WriteLine(
                        $"[{reader["id"]}] (Гриф: {reader["secret_level"]}) {reader["title"]}"
                    );

                    Console.WriteLine($"Содержание: {reader["content"]}\n");
                }

                if (!found)
                {
                    Console.WriteLine("Документы не найдены.");
                }
            }
            catch
            {
                Console.WriteLine("[ОШИБКА] Не удалось выполнить поиск документов.");
            }
        }
    }
}