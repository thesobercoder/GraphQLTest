using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQL.Api
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerID { get; set; }
        public ICollection<Customer> Customers { get; set; }
    }

    public class CustomerGraph : EfObjectGraphType<TestDBContext, Customer>
    {
        public CustomerGraph(IEfGraphQLService<TestDBContext> graphQlService) :
            base(graphQlService)
        {
            Field(x => x.CustomerID);
            Field(x => x.CustomerName);
            AddNavigationListField(
                name: "orders",
                resolve: context => context.Source.Orders);
        }
    }

    public class OrderGraph : EfObjectGraphType<TestDBContext, Order>
    {
        public OrderGraph(IEfGraphQLService<TestDBContext> graphQlService) :
            base(graphQlService)
        {
            Field(x => x.OrderID);
            Field(x => x.OrderDate);
            AddNavigationListField(
                name: "customers",
                resolve: context => context.Source.Customers);
        }
    }

    public class TestDBContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        public TestDBContext(DbContextOptions<TestDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder != null)
            {
                builder.Entity<Customer>().ToTable("Customers");
                builder.Entity<Customer>().HasKey(x => x.CustomerID);

                builder.Entity<Order>().ToTable("Orders");
                builder.Entity<Order>().HasKey(x => x.OrderID);

                builder.Entity<Order>().HasMany(x => x.Customers);
                builder.Entity<Customer>().HasMany(x => x.Orders);
            }
        }

        static IModel BuildStaticModel()
        {
            var builder = new DbContextOptionsBuilder<TestDBContext>();
            builder.UseSqlServer("Fake");
            using var dbContext = new TestDBContext(builder.Options);
            return dbContext.Model;
        }

        public static IModel StaticModel { get; } = BuildStaticModel();
    }

    public class SchemaTest : Schema
    {
        public SchemaTest(IServiceProvider resolver) : base(resolver)
        {
            Query = resolver.GetService<QueryTest>();
        }
    }

    public class QueryTest : QueryGraphType<TestDBContext>
    {
        public QueryTest(IEfGraphQLService<TestDBContext> graphQlService) :
            base(graphQlService)
        {
            Name = "Query";
            AddQueryField(
                name: "customers",
                resolve: context => context.DbContext.Customers
            );
            AddQueryField(
                name: "orders",
                resolve: context => context.DbContext.Orders
            );
        }
    }
}

