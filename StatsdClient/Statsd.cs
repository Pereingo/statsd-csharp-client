using System;
using System.Collections.Generic;
using System.Data;

namespace StatsdClient
{
    public class Statsd : IStatsd
    {
        private IStatsdUDP Udp { get; set; }
        private IRandomGenerator RandomGenerator { get; set; }
        public List<string> Commands { get { return _commands; } private set { _commands = value; } }
        private List<string> _commands = new List<string>();

        public class Counting : ICommandType {}
        public class Timing : ICommandType { }
        public class Gauge : ICommandType { }

        private readonly Dictionary<Type, string> _commandToUnit = new Dictionary<Type, string> { { typeof(Counting), "c" }, { typeof(Timing), "ms" }, { typeof(Gauge), "g" } };

        public Statsd(IStatsdUDP udp, IRandomGenerator randomGenerator)
        {
            Udp = udp;
            RandomGenerator = randomGenerator;
            
        }

        public Statsd(IStatsdUDP udp)
        {
            Udp = udp;
            RandomGenerator = new RandomGenerator();
        }
      
        public void Send<TCommandType>(string name, int value) where TCommandType : ICommandType
        {
            Send<TCommandType>(name, value, 1);
        }

        public void Add<TCommandType>(string name, int value) where TCommandType : ICommandType
        {
            _commands.Add(GetCommand(name, value, _commandToUnit[typeof(TCommandType)], 1));
        }
      
        public void Send<TCommandType>(string name, int value, double sampleRate) where TCommandType : ICommandType
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Send(GetCommand(name, value, _commandToUnit[typeof (TCommandType)], sampleRate));
            };
        }

        public void Add<TCommandType>(string name, int value, double sampleRate) where TCommandType : ICommandType
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                _commands.Add(GetCommand(name, value, _commandToUnit[typeof (TCommandType)], sampleRate));
            }
        }
   
        private void Send(string command)
        {
            Commands = new List<string>() {command};
            Send();
        }

        public void Send()
        {
            try
            {
                Udp.Send(string.Join(Environment.NewLine, Commands.ToArray()));
                Commands = new List<string>();
            }
            catch
            {
                //metrics fail is silent
            }
        }

        private static string GetCommand(string name, int value, string unit, double sampleRate)
        {
            string format = sampleRate == 1 ? "{0}:{1}|{2}" : "{0}:{1}|{2}|@{3}";
            return string.Format(format, name, value, unit, sampleRate);
        }
    }
}
