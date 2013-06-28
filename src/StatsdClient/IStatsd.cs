using System;
using System.Collections.Generic;

namespace StatsdClient
{
    public interface IStatsd
    {
        List<string> Commands { get; }
        void Send<TCommandType,T>(string name, T value, double sampleRate, params string[] tags) where TCommandType : ICommandType;
        void Add<TCommandType,T>(string name, T value, double sampleRate, params string[] tags) where TCommandType : ICommandType;
        void Send(string command);
        void Send();
        void Add(Action actionToTime, string statName, double sampleRate, params string[] tags);
        void Send(Action actionToTime, string statName, double sampleRate, params string[] tags);
    }
}