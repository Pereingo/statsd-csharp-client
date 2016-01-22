using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatsdClient
{
    public partial interface IStatsd
    {
        Task SendAsync<TCommandType>(string name, long value) where TCommandType : IAllowsInteger;

        Task SendAsync<TCommandType>(string name, double value) where TCommandType : IAllowsDouble;

        Task SendAsync<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta;

        Task SendAsync<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate;

        Task SendAsync<TCommandType>(string name, string value) where TCommandType : IAllowsString;

        Task SendAsync();

        Task SendAsync(Action actionToTime, string statName, double sampleRate=1);
    }
}