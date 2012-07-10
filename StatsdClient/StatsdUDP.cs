using System.Net.Sockets;
using System.Text;

namespace StatsdClient
{
    public class StatsdUDP : IStatsdUDP
    {
        private string Name { get; set; }
        private int Port { get; set; }
        private UdpClient UDPClient { get; set; }

        public StatsdUDP(string name, int port)
        {
            Name = name;
            Port = port;
            UDPClient = new UdpClient(Name, Port);           
        }

        public void Send(string command)
        {
            byte[] encodedCommand = Encoding.ASCII.GetBytes(command);
            UDPClient.Send(encodedCommand, encodedCommand.Length);
            UDPClient.Close();
        }
    }
}