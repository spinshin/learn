using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using QuartzTest;

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((cxt, services) =>
    {
        services.AddQuartz();
        services.AddScoped<MyDbContext>();
        services.AddQuartzHostedService(opt => { opt.WaitForJobsToComplete = true; });
    }).Build();

var schedulerFactory = builder.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

var jobKey = new JobKey("myJob", "group1");

var job = JobBuilder.Create<HelloJob>()
    .WithIdentity(jobKey)
    .StoreDurably()
    .Build();

await scheduler.AddJob(job, false);

var minute = 40;

var trigger = TriggerBuilder.Create()
    .ForJob(jobKey)
    .WithIdentity("myTrigger", "group1")
    .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(21, minute, DayOfWeek.Friday))
    .Build();

var trigger2 = TriggerBuilder.Create()
    .ForJob(jobKey)
    .WithIdentity("myTrigger2", "group1")
    .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(21, minute, DayOfWeek.Friday))
    .Build();

await scheduler.ScheduleJob(trigger);
// await scheduler.ScheduleJob(trigger2);
await builder.RunAsync();
