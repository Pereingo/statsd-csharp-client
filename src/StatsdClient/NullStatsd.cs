using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatsdClient
{
    //this will disable the warnings that say await is missing. It is intentionally done because it is nullstatsd.
#pragma warning disable 1998
    public class NullStatsd : IStatsd
    {
        public NullStatsd()
        {
            Commands = new List<string>();
        }

        public List<string> Commands { get; private set; }

#if !NET451
        public async Task SendAsync<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
        }

        public async Task SendAsync<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
        }

        public async Task SendAsync<TCommandType>(string name, long value, double sampleRate)
            where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
        }

        public async Task SendAsync<TCommandType>(string name, string value) where TCommandType : IAllowsString
        {
        }

        public async Task SendAsync()
        {
        }

        public async Task SendAsync<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta
        {
        }

        public async Task SendAsync(Func<Task> actionToTime, string statName, double sampleRate = 1)
        {
            await actionToTime();
        }
#endif

        public void Send<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
        }

        public void Add<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
        }

        public void Send<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
        }

        public void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
        }

        public void Send<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta
        {
        }

        public void Send<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
        }

        public void Add<TCommandType>(string name, long value, double sampleRate)
            where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
        }

        public void Send<TCommandType>(string name, string value) where TCommandType : IAllowsString
        {
        }

        public void Send()
        {
        }

        public void Add(Action actionToTime, string statName, double sampleRate = 1)
        {
            actionToTime();
        }

        public void Send(Action actionToTime, string statName, double sampleRate = 1)
        {
            actionToTime();
        }
    }
#pragma warning restore 1998
}