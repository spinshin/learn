using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace QuartzTest;

public class HelloJob : IJob
{
    private readonly MyDbContext _dbContext;
    private readonly IServiceProvider _sp;

    public HelloJob(MyDbContext dbContext, IServiceProvider sp)
    {
        _dbContext = dbContext;
        _sp = sp;
    }

    public Task Execute(IJobExecutionContext context)
    {
        if (context.RefireCount > 2)
            return Task.CompletedTask;

        try
        {
            Console.WriteLine($"Hello {_dbContext.Id}");
            throw new Exception();
        }
        catch (Exception ex)
        {
            throw new JobExecutionException("Job failed.", refireImmediately: true, cause: ex);
        }
    }
}

public class MyDbContext
{
    public Guid Id { get; } = Guid.NewGuid();
}
