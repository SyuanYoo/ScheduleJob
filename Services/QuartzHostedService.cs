using Quartz.Spi;
using Quartz;
using ScheduleJob.Services.Jobs;
using System.Collections.Concurrent;

namespace ScheduleJob.Services
{
    /// <summary>
    /// 託管排程
    /// </summary>
    public class QuartzHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, string> _currentCronExpressions = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="configuration"></param>
        public QuartzHostedService(IScheduler scheduler, IConfiguration configuration)
        {
            _scheduler = scheduler;
            _configuration = configuration;
        }

        /// <summary>
        /// 服務啟動時執行
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            #region 訂閱排程
            await ScheduleJob<SampleJob1>("SampleJob1", cancellationToken);
            await ScheduleJob<SampleJob2>("SampleJob2", cancellationToken);
            #endregion

            //訂閱appsettings異動
            ((IConfigurationRoot)_configuration).GetReloadToken().RegisterChangeCallback(async _ =>
            {
                foreach (var jobKey in _currentCronExpressions.Keys)
                {
                    //檢核當CRON異動時重新訂閱排程
                    var newCronExpression = _configuration[$"QuartzConfig:{jobKey}"];
                    var currentCronExpression = _currentCronExpressions[jobKey];
                    if (newCronExpression != currentCronExpression)
                    {
                        await RescheduleJob(jobKey, newCronExpression);
                        _currentCronExpressions[jobKey] = newCronExpression;
                    }
                }
            }, null);

            await _scheduler.Start(cancellationToken);
        }

        /// <summary>
        /// 訂閱排程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobKeyName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ScheduleJob<T>(string jobKeyName, CancellationToken cancellationToken) where T : IJob
        {
            var job = JobBuilder.Create<T>().WithIdentity(jobKeyName).Build();
            var cronExpression = _configuration[$"QuartzConfig:{jobKeyName}"];
            _currentCronExpressions[jobKeyName] = cronExpression;

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"TriggerFor{jobKeyName}")
                .WithCronSchedule(cronExpression)
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        /// <summary>
        /// 服務停止時執行
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //關閉排程(默認當前作業執行完成後才會完全停止)
            await _scheduler?.Shutdown(cancellationToken);
        }

        /// <summary>
        /// 重新訂閱排程
        /// </summary>
        /// <param name="jobKeyName"></param>
        /// <param name="newCronExpression"></param>
        /// <returns></returns>
        private async Task RescheduleJob(string jobKeyName, string newCronExpression)
        {
            var triggerKey = new TriggerKey($"TriggerFor{jobKeyName}");
            var newTrigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithCronSchedule(newCronExpression)
                .Build();
            await _scheduler.RescheduleJob(triggerKey, newTrigger);
        }
    }
}
