using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatsdClient
{
    public interface IStatsd
    {
        List<string> Commands { get; }

        Task SendAsync<TCommandType>(string name, long value) where TCommandType : IAllowsInteger;
        void Add<TCommandType>(string name, long value) where TCommandType : IAllowsInteger;

        Task SendAsync<TCommandType>(string name, double value) where TCommandType : IAllowsDouble;
        void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble;
        Task SendAsync<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta;

        Task SendAsync<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate;
        void Add<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate;

        Task SendAsync<TCommandType>(string name, string value) where TCommandType : IAllowsString;

        Task SendAsync();

        void Add(Action actionToTime, string statName, double sampleRate=1);
        Task SendAsync(Action actionToTime, string statName, double sampleRate=1);
    }
}