using Quartz.Spi;
using Quartz;

namespace ScheduleJob.Services.JobFactory
{
    /// <summary>
    /// 提供和管理作業IJob的實例。
    /// 當Trigger被觸發時，透過該工廠方法取得作業實例
    /// </summary>
    public class QuartzJobFactory : IJobFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceScopeFactory"></param>
        public QuartzJobFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// 當 IScheduler 需要一個 IJob 實例來執行時，會調用此方法
        /// </summary>
        /// <param name="bundle">包含與即將執行的作業相關的資訊，例如作業詳情、觸發器、日誌等</param>
        /// <param name="scheduler">調用此作業的排程器實例</param>
        /// <returns></returns>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            return job;
        }

        /// <summary>
        /// 當 IScheduler 完成對 IJob 的調用後，此方法會被觸發，通常用於進行清理工作，如釋放資源或進行其他後續處理。
        /// </summary>
        /// <param name="job">剛完成執行的作業的實例</param>
        public void ReturnJob(IJob job) { }
    }
}
