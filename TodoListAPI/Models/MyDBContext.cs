using Microsoft.EntityFrameworkCore;

namespace TodoListAPI.Models
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions options) : base(options) { }

        #region DbSet
        public DbSet<User> Users { get; set; }
        public DbSet<Todo> Todos { get; set; }

        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("User");
                e.HasKey(pk => pk.Id);
                e.HasIndex(u => u.Email).IsUnique();

                // password = Thanh123@
                e.HasData(new User { Id = 1, Email = "Thanh123@gmail.com", Name = "Thanh", Password = "$2a$11$0/CP8hh.odVCJCJi0d261ObBVpXQ06FuX53Aiq6Fn.0pKKdcdnMz2" });
            });

            modelBuilder.Entity<Todo>(e =>
            {
                e.ToTable("Todo");
                e.HasKey(pk => pk.Id);
                e.Property(r => r.CreatedDate)
                    .HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd(); // Chỉ tạo giá trị khi thêm mới
                e.Property(r => r.UpdatedDate);

                e.HasOne(t => t.User)
                    .WithMany(u => u.Todos)
                    .HasForeignKey(f => f.UserId);
            });
        }

    }
}
