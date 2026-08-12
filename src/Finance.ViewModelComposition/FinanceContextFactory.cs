using Microsoft.EntityFrameworkCore;
using ITOps.Infrastructure;
using System;

namespace Finance.ViewModelComposition
{
    public static class FinanceContextFactory
    {
        public static Func<Data.FinanceContext> Create()
        {
            return () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Data.FinanceContext>();
                
                // 🐋 Pulls the specific finance database connection key from ITOps
                var connectionString = ConnectionStringProvider.GetConnectionString("finance-db");
                optionsBuilder.UseNpgsql(connectionString);
                
                return new Data.FinanceContext(optionsBuilder.Options);
            };
        }
    }
}
