using Microsoft.Extensions.DependencyInjection;
using System;

namespace ScopedResolutionDemo
{
    // Simulate a DbContext with a unique ID
    public class MyDbContext
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public class ServiceB
    {
        public MyDbContext DbContext { get; }

        public ServiceB(MyDbContext dbContext)
        {
            DbContext = dbContext;
        }
    }

    public class ServiceA
    {
        private readonly IServiceProvider _provider;
        public MyDbContext DbContext { get; set; }

        public ServiceA(IServiceProvider provider, MyDbContext dbContext)
        {
            _provider = provider;
            DbContext = dbContext;
        }

        public void Run()
        {
            var b1 = _provider.GetRequiredService<ServiceB>();
            var b2 = _provider.GetRequiredService<ServiceB>();

            Console.WriteLine($"A1 DbContext: {DbContext.Id}");
            Console.WriteLine($"B1 DbContext: {b1.DbContext.Id}");
            Console.WriteLine($"B2 DbContext: {b2.DbContext.Id}");
        }
    }

    class Test
    {
        public static void Run(IServiceProvider provider)
        {
            // First scope
            Console.WriteLine("Scope 1:");
            var a1 = provider.GetRequiredService<ServiceA>();
            a1.Run();

            Console.WriteLine();

            Console.WriteLine("Scope 2:");
            var a2 = provider.GetRequiredService<ServiceA>();
            a2.Run();
        }
    }

    class Program
    {
        public static void Main()
        {
            var services = new ServiceCollection();

            services.AddScoped<MyDbContext>(); // Scoped
            services.AddTransient<ServiceB>(); // Transient
            services.AddTransient<ServiceA>(); // Transient

            var provider = services.BuildServiceProvider();

            Test.Run(provider);

            Console.WriteLine();

            Test.Run(provider);
        }
    }
}
