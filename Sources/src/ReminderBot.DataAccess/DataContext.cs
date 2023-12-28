using Microsoft.EntityFrameworkCore;
using ReminderBot.Entities;

namespace ReminderBot.DataAccess
{
    public class DataContext : DbContext
    {
        public const string SchemaName = "rd";

        private static readonly object _locker = new object();
        private static bool _isInitialized = false;

        public DataContext() { }

        public DataContext(DbContextOptions<DataContext> options) : base(options){ }

        public virtual DbSet<TelegramUser> TelegramUsers { get; set; }
        public virtual DbSet<TelegramUserSettings> TelegramUserSettings { get; set; }
        public virtual DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        }

        public void Init()
        {
            lock (_locker)
            {
                if (_isInitialized) return;

                this.Database.Migrate();
                this.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

                _isInitialized = true;
            }
        }
    }
}
