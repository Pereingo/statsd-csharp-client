using System;
using System.Collections.Generic;

namespace StatsdClient
{
    public interface IStatsd
    {
        List<string> Commands { get; }
        
        //Generic Send and Add
        void Send<TCommandType>(string name, int value) where TCommandType : ICommandType;
        void Add<TCommandType>(string name, int value) where TCommandType : ICommandType;

        //Counting, Timing specific methods, allow sampleRate
        void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : ICommandType, IAllowsSampleRate;
        void Add<TCommandType>(string name, int value, double sampleRate) where TCommandType : ICommandType, IAllowsSampleRate;  
                
        void Send(string command);        
        void Send();
        
        void Add(Action actionToTime, string statName);
        void Send(Action actionToTime, string statName);
    }
}