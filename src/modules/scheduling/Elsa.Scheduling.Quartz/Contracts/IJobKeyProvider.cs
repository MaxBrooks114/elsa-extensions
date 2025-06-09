using Quartz;

namespace Elsa.Scheduling.Quartz.Contracts;

internal interface IJobKeyProvider
{
    JobKey GetJobKey<TJob>() where TJob : IJob;
    string GetGroupName();
}