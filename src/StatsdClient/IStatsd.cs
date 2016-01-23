using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatsdClient
{
    public partial interface IStatsd
    {
#if !NET451

        Task SendAsync<TCommandType>(string name, long value) where TCommandType : IAllowsInteger;

        Task SendAsync<TCommandType>(string name, double value) where TCommandType : IAllowsDouble;

        Task SendAsync<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta;

        Task SendAsync<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate;

        Task SendAsync<TCommandType>(string name, string value) where TCommandType : IAllowsString;

        Task SendAsync();

        Task SendAsync(Func<Task> actionToTime, string statName, double sampleRate=1);

        Task<T> SendAsync<T>(Func<Task<T>> actionToTime, string statName, double sampleRate = 1);
#endif
    }
}