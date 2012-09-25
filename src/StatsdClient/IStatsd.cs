using System;
using System.Collections.Generic;

namespace StatsdClient
{
    public interface IStatsd
    {
        List<string> Commands { get; }
        void Send<TCommandType>(string name, int value) where TCommandType : ICommandType;
        void Send(string name, int value, double sampleRate);
        void Add<TCommandType>(string name, int value) where TCommandType : ICommandType;
        void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : ICommandType;
        void Add(string name, int value, double sampleRate);
        void Send(string command);
        void Send();
        void Add(Action actionToTime, string statName);
        void Send(Action actionToTime, string statName);
    }
}