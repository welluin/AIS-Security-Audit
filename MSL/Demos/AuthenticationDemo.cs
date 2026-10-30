using MSL.Data;
using MSL.Models;

namespace MSL.Demos
{
    public static class AuthenticationDemo
    {
        public static void Run()
        {
            UserSession.CurrentEmployee = null;

            Console.Write("\nВведите логин сотрудника: ");
            string? login = Console.ReadLine();

            Console.Write("Введите пароль: ");
            string? password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrEmpty(password))
            {
                Console.WriteLine("\n[ОТКАЗ] Необходимо заполнить все поля.");
                return;
            }

            using var connection = DatabaseContext.CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT id, login, password_hash, password_salt,
                       full_name, department_id, clearance_level
                FROM Employees
                WHERE login = @login;
            ";

            command.Parameters.AddWithValue("@login", login);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                Console.WriteLine("\n[ОТКАЗ] Неверные учетные данные.");
                return;
            }

            string storedHash = reader["password_hash"].ToString()!;
            string storedSalt = reader["password_salt"].ToString()!;

            if (!PasswordHasher.Verify(password, storedHash, storedSalt))
            {
                Console.WriteLine("\n[ОТКАЗ] Неверные учетные данные.");
                return;
            }

            var employee = new Employee
            {
                Id = Convert.ToInt32(reader["id"]),
                Login = reader["login"].ToString()!,
                PasswordHash = storedHash,
                PasswordSalt = storedSalt,
                FullName = reader["full_name"].ToString()!,
                DepartmentId = Convert.ToInt32(reader["department_id"]),
                ClearanceLevel = Convert.ToInt32(reader["clearance_level"])
            };

            reader.Close();

            UserSession.CurrentEmployee = employee;

            Console.WriteLine($"\n[АВТОРИЗОВАН] {employee.FullName}");
            Console.WriteLine($"Отдел: ID {employee.DepartmentId}");
            Console.WriteLine($"Допуск: Уровень {employee.ClearanceLevel}");
        }
    }
}