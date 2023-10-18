using Quartz;

namespace ScheduleJob.Services.Jobs
{
    /// <summary>
    /// 繼承IJob並實作Execute
    /// </summary>
    public class SampleJob2 : IJob
    {
        private readonly ILogger<SampleJob2> _logger;
        public SampleJob2(ILogger<SampleJob2> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 實際要執行的程式
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            //TODO 實際執行作業、LOG機制、例外處理...
            _logger.LogInformation($"SampleJob2 executed at: {DateTime.Now}");
        }
    }
}
