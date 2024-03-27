using up_repo_clone;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddQuartz(q =>
{
    q.AddJob<Worker>(j => j.WithIdentity("job1"));
    q.AddTrigger(t => t
        .ForJob("job1")
        .WithIdentity("trigger1")
        .StartNow()
        .WithSimpleSchedule(s => s.WithIntervalInSeconds(60).RepeatForever()));
});
builder.Services.AddQuartzHostedService(q =>
{
    q.WaitForJobsToComplete = true;
});
var host = builder.Build();
host.Run();
