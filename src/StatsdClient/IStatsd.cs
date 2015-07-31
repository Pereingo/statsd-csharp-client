using StatsdClient.MetricTypes;
using System;
using System.Collections.Generic;

namespace StatsdClient
{
    public interface IStatsd
    {
        void Send<TCommandType>(string name, int value) where TCommandType : Metric, IAllowsInteger, new();
        void Send<TCommandType>(string name, double value) where TCommandType : Metric, IAllowsDouble, new();
        void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : Metric, IAllowsInteger, IAllowsSampleRate, new();
        void Send<TCommandType>(string name, string value) where TCommandType : Metric, IAllowsString, new();
        void Send(Action actionToTime, string statName, double sampleRate=1);
    }
}