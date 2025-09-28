using SQLite;
using KnowExpends.Mobile.Models;

namespace KnowExpends.Mobile.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "expenses.db");
            _database = new SQLiteAsyncConnection(databasePath);
            await _database.CreateTableAsync<Expense>();
        }

        public async Task<List<Expense>> GetExpensesAsync()
        {
            if (_database == null)
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "expenses.db");
                _database = new SQLiteAsyncConnection(databasePath);
            }
            
            return await _database.Table<Expense>().OrderByDescending(x => x.CreatedDate).ToListAsync();
        }

        public async Task<int> SaveExpenseAsync(Expense expense)
        {
            if (_database == null)
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "expenses.db");
                _database = new SQLiteAsyncConnection(databasePath);
            }
            
            return await _database.InsertAsync(expense);
        }

        public async Task<int> UpdateExpenseAsync(Expense expense)
        {
            if (_database == null)
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "expenses.db");
                _database = new SQLiteAsyncConnection(databasePath);
            }
            
            return await _database.UpdateAsync(expense);
        }

        public async Task<int> DeleteExpenseAsync(Expense expense)
        {
            if (_database == null)
            {
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "expenses.db");
                _database = new SQLiteAsyncConnection(databasePath);
            }
            
            return await _database.DeleteAsync(expense);
        }
    }
}


