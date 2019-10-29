using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Models
{
    public class BankAccountContext : DbContext
    {
        public BankAccountContext (DbContextOptions options) : base(options) {}

        //property will be a DbSet - a collection type from the Entity Framework library 
        //that you will provide your Model class in angle brackets
        //Your DBSet will refer to all data in your corresponding table as objects of the Model type you provide.
        public DbSet<User> Users {get; set;}
        public DbSet<Transaction> Transactions {get; set;}
    }
}