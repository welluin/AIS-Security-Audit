using Microsoft.Data.Sqlite;

namespace MSL.Data
{
    public static class DatabaseInitializer
    {
        public static string GetDdl()
        {
            return @"
                -- Таблица подразделений
                CREATE TABLE IF NOT EXISTS Departments (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE
                );

                -- Таблица сотрудников
                CREATE TABLE IF NOT EXISTS Employees (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    login TEXT NOT NULL UNIQUE,
                    password_hash TEXT NOT NULL,
                    password_salt TEXT NOT NULL,
                    full_name TEXT NOT NULL,
                    department_id INTEGER NOT NULL,
                    clearance_level INTEGER NOT NULL
                        CHECK (clearance_level IN (1, 2, 3)),
                    FOREIGN KEY (department_id)
                        REFERENCES Departments(id)
                        ON DELETE RESTRICT
                );

                -- Отдельное хранение персональных данных сотрудников
                CREATE TABLE IF NOT EXISTS EmployeePersonalData (
                    employee_id INTEGER PRIMARY KEY,
                    passport_data TEXT NOT NULL,
                    FOREIGN KEY (employee_id)
                        REFERENCES Employees(id)
                        ON DELETE CASCADE
                );

                -- Таблица документов
                CREATE TABLE IF NOT EXISTS ClassifiedDocuments (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    content TEXT NOT NULL,
                    secret_level INTEGER NOT NULL
                        CHECK (secret_level IN (1, 2, 3)),
                    author_id INTEGER NOT NULL,
                    department_id INTEGER NOT NULL,
                    FOREIGN KEY (author_id)
                        REFERENCES Employees(id)
                        ON DELETE RESTRICT,
                    FOREIGN KEY (department_id)
                        REFERENCES Departments(id)
                        ON DELETE RESTRICT
                );

                -- Таблица обращений граждан
                CREATE TABLE IF NOT EXISTS CitizenRequests (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    citizen_fio TEXT NOT NULL,
                    request_text TEXT NOT NULL,
                    assigned_employee_id INTEGER,
                    status TEXT NOT NULL
                        CHECK (status IN (
                            'Новое',
                            'В работе',
                            'В обработке',
                            'Завершено',
                            'Отклонено'
                        )),
                    FOREIGN KEY (assigned_employee_id)
                        REFERENCES Employees(id)
                        ON DELETE RESTRICT
                );

                -- Журнал аудита
                CREATE TABLE IF NOT EXISTS AuditLog (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    action_time TEXT NOT NULL,
                    user_login TEXT NOT NULL,
                    action_description TEXT NOT NULL
                );
            ";
        }

        public static void Initialize()
        {
            using var connection = DatabaseContext.CreateConnection();

            using var command = connection.CreateCommand();
            command.CommandText = GetDdl();
            command.ExecuteNonQuery();

            command.CommandText = "SELECT COUNT(*) FROM Employees;";
            long count = (long)(command.ExecuteScalar() ?? 0);

            if (count > 0)
            {
                return;
            }

            using var transaction = connection.BeginTransaction();

            try
            {
                using var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;

                insertCommand.CommandText = @"
                    INSERT INTO Departments (name) VALUES
                    ('Департамент Специальной Связи'),
                    ('Аналитический Департамент'),
                    ('Общий Отдел');
                ";
                insertCommand.ExecuteNonQuery();

                AddEmployee(
                    connection,
                    transaction,
                    "minister",
                    "SuperMinister2026!",
                    "Воронов В.В.",
                    1,
                    3,
                    "4510 998877"
                );

                AddEmployee(
                    connection,
                    transaction,
                    "analyst_petrov",
                    "petrovPass",
                    "Петров П.П.",
                    2,
                    2,
                    "4512 112233"
                );

                AddEmployee(
                    connection,
                    transaction,
                    "clerk_sidorov",
                    "123456",
                    "Сидоров С.С.",
                    3,
                    1,
                    "4515 445566"
                );

                insertCommand.CommandText = @"
                    INSERT INTO ClassifiedDocuments
                    (title, content, secret_level, author_id, department_id)
                    VALUES
                    (
                        'Спецификация квантовой криптографии',
                        'Секретные частоты и узлы: 144.800 MHz...',
                        3, 1, 1
                    ),
                    (
                        'Анализ угроз информбезопасности',
                        'Выявлены сетевые аномалии в подсети B...',
                        2, 2, 2
                    ),
                    (
                        'График дежурств охраны',
                        'Список ответственных за КПП...',
                        1, 3, 3
                    );
                ";
                insertCommand.ExecuteNonQuery();

                insertCommand.CommandText = @"
                    INSERT INTO CitizenRequests
                    (citizen_fio, request_text, assigned_employee_id, status)
                    VALUES
                    ('Иванов И.И.', 'Запрос на выдачу сертификата ключа', 3, 'В работе'),
                    ('Алексеев А.А.', 'Жалоба на задержку ответа', 2, 'Новое');
                ";
                insertCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static void AddEmployee(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string login,
            string password,
            string fullName,
            int departmentId,
            int clearanceLevel,
            string passportData)
        {
            string passwordHash = PasswordHasher.CreateHash(password, out string passwordSalt);

            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = @"
                INSERT INTO Employees
                (login, password_hash, password_salt, full_name, department_id, clearance_level)
                VALUES
                (@login, @passwordHash, @passwordSalt, @fullName, @departmentId, @clearanceLevel);
            ";

            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            command.Parameters.AddWithValue("@passwordSalt", passwordSalt);
            command.Parameters.AddWithValue("@fullName", fullName);
            command.Parameters.AddWithValue("@departmentId", departmentId);
            command.Parameters.AddWithValue("@clearanceLevel", clearanceLevel);

            command.ExecuteNonQuery();

            using var idCommand = connection.CreateCommand();
            idCommand.Transaction = transaction;
            idCommand.CommandText = "SELECT last_insert_rowid();";

            long employeeId = (long)idCommand.ExecuteScalar()!;
            
            using var personalDataCommand = connection.CreateCommand();
            personalDataCommand.Transaction = transaction;

            personalDataCommand.CommandText = @"
                INSERT INTO EmployeePersonalData (employee_id, passport_data)
                VALUES (@employeeId, @passportData);
            ";

            personalDataCommand.Parameters.AddWithValue("@employeeId", employeeId);
            personalDataCommand.Parameters.AddWithValue("@passportData", passportData);

            personalDataCommand.ExecuteNonQuery();
        }
    }
}