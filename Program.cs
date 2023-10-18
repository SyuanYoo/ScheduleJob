using Quartz.Impl;
using ScheduleJob.Services.JobFactory;
using ScheduleJob.Services.Jobs;
using ScheduleJob.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<QuartzJobFactory>();
        services.AddTransient<SampleJob1>();
        services.AddTransient<SampleJob2>();

        services.AddSingleton(provider =>
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = schedulerFactory.GetScheduler().Result;
            scheduler.JobFactory = provider.GetService<QuartzJobFactory>();
            return scheduler;
        });
        services.AddHostedService<QuartzHostedService>();
    })
    .Build();

await host.RunAsync();
