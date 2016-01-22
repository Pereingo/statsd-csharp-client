using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatsdClient
{
    public partial interface IStatsd
    {
        List<string> Commands { get; }

        void Send<TCommandType>(string name, long value) where TCommandType : IAllowsInteger;

        void Add<TCommandType>(string name, long value) where TCommandType : IAllowsInteger;

        void Send<TCommandType>(string name, double value) where TCommandType : IAllowsDouble;

        void Add<TCommandType>(string name, double value) where TCommandType : IAllowsDouble;

        void Send<TCommandType>(string name, double value, bool isDeltaValue) where TCommandType : IAllowsDouble, IAllowsDelta;

        void Send<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate;

        void Add<TCommandType>(string name, long value, double sampleRate) where TCommandType : IAllowsInteger, IAllowsSampleRate;

        void Send<TCommandType>(string name, string value) where TCommandType : IAllowsString;

        void Send();

        void Add(Action actionToTime, string statName, double sampleRate=1);

        void Send(Action actionToTime, string statName, double sampleRate=1);
    }
}