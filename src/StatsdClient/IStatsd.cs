using System;
using System.Collections.Generic;

namespace StatsdClient
{
    public interface IStatsd
    {
        List<string> Commands { get; }
        void Send<TCommandType>(string name, int value, params string[] tags) where TCommandType : ICommandType;
        void Add<TCommandType>(string name, int value, params string[] tags) where TCommandType : ICommandType;
        void Send<TCommandType>(string name, int value, double sampleRate, params string[] tags) where TCommandType : ICommandType;
        void Add<TCommandType>(string name, int value, double sampleRate, params string[] tags) where TCommandType : ICommandType;
        void Send(string command);
        void Send();
        void Add(Action actionToTime, string statName, params string[] tags);
        void Send(Action actionToTime, string statName, params string[] tags);
    }
}