using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatsdClient
{
    public class NullStatsd : IStatsd
    {
        public NullStatsd()
        {
            Commands = new List<string>();
        }

        public List<string> Commands { get; private set; }

        public async Task SendAsync<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
        }

        public void Add<TCommandType>(string name, long value) where TCommandType : IAllowsInteger
        {
        }

        public async Task SendAsync<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
        }

        public void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble
        {
        }

        public async Task SendAsync<TCommandType>(string name, long value, double sampleRate)
            where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
        }

        public void Add<TCommandType>(string name, long value, double sampleRate)
            where TCommandType : IAllowsInteger, IAllowsSampleRate
        {
        }

        public async Task SendAsync<TCommandType>(string name, string value) where TCommandType : IAllowsString
        {
        }

        public async Task SendAsync()
        {
        }

        public void Add(Action actionToTime, string statName, double sampleRate = 1)
        {
            actionToTime();
        }

        public async Task SendAsync(Action actionToTime, string statName, double sampleRate = 1)
        {
            actionToTime();
        }

        public async Task SendAsync<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta
        {
        }
  }
}