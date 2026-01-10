using Microsoft.EntityFrameworkCore;

namespace El_Buen_Taco.Data;

public class DataProtectionKeysContext : DbContext
{
    public DataProtectionKeysContext(DbContextOptions<DataProtectionKeysContext> options) : base(options) { }

    public DbSet<DataProtectionKeyEntity> DataProtectionKeys { get; set; } = null!;
}
