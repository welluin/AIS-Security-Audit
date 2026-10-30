using MSL.Data;

namespace MSL.Demos
{
    public static class CitizenServiceDemo
    {
        public static void Run()
        {
            if (UserSession.CurrentEmployee == null)
            {
                Console.WriteLine("\n[ОТКАЗ] Сначала необходимо авторизоваться.");
                return;
            }

            Console.Write("\nФИО гражданина: ");
            string? fio = Console.ReadLine();

            Console.Write("Суть обращения: ");
            string? requestText = Console.ReadLine();

            Console.Write("Назначить сотрудника (ID, можно оставить пустым): ");
            string? empIdInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(fio) ||
                string.IsNullOrWhiteSpace(requestText))
            {
                Console.WriteLine("\n[ОШИБКА] Необходимо заполнить данные обращения.");
                return;
            }

            int? employeeId = null;

            if (!string.IsNullOrWhiteSpace(empIdInput))
            {
                if (!int.TryParse(empIdInput, out int parsedId) || parsedId <= 0)
                {
                    Console.WriteLine("\n[ОШИБКА] Некорректный ID сотрудника.");
                    return;
                }

                employeeId = parsedId;
            }

            using var connection = DatabaseContext.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var cmdInsert = connection.CreateCommand();
                cmdInsert.Transaction = transaction;

                cmdInsert.CommandText = @"
                    INSERT INTO CitizenRequests
                    (citizen_fio, request_text, assigned_employee_id, status)
                    VALUES
                    (@fio, @requestText, @employeeId, 'В работе');
                ";

                cmdInsert.Parameters.AddWithValue("@fio", fio);
                cmdInsert.Parameters.AddWithValue("@requestText", requestText);

                cmdInsert.Parameters.AddWithValue(
                    "@employeeId",
                    employeeId.HasValue ? employeeId.Value : DBNull.Value
                );

                cmdInsert.ExecuteNonQuery();

                using var cmdAudit = connection.CreateCommand();
                cmdAudit.Transaction = transaction;

                cmdAudit.CommandText = @"
                    INSERT INTO AuditLog
                    (action_time, user_login, action_description)
                    VALUES
                    (@actionTime, @userLogin, @description);
                ";

                cmdAudit.Parameters.AddWithValue(
                    "@actionTime",
                    DateTime.UtcNow.ToString("o")
                );

                cmdAudit.Parameters.AddWithValue(
                    "@userLogin",
                    UserSession.CurrentEmployee.Login
                );

                cmdAudit.Parameters.AddWithValue(
                    "@description",
                    "Создано обращение гражданина: " + fio
                );

                cmdAudit.ExecuteNonQuery();

                transaction.Commit();

                Console.WriteLine("\n[УСПЕХ] Обращение успешно зафиксировано.");
            }
            catch
            {
                transaction.Rollback();

                Console.WriteLine(
                    "\n[ОШИБКА] Не удалось сохранить обращение. Изменения отменены."
                );
            }
        }
    }
}