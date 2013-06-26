using System;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Text;

namespace Tests
{
    namespace Helpers
    {
        // A small UDP server that can be used for testing.
        // Stores the last message that was received by the server
        // until it is accessed using GetAndClearLastMessage().

        // By design received messages can only be read once. This
        // allows one instance of the listener to be used across
        // multiple tests without risk of the results of previous tests
        // affecting the current one.

        // Intended use:
        // udpListener = new UdpListener(serverName, serverPort);
        // listenThread = new Thread(new ThreadStart(udpListener.Listen));
        // listenThread.Start();
        // { send something to the listener }
        // while(listenThread.IsAlive); // wait for listen thread to receive message or time out
        // received_message = udpListener.GetAndClearLastMessage()
        // { make sure that the received message is what was expected }
        public class UdpListener : IDisposable 
        {
            byte[] lastReceivedBytes;
            IPEndPoint localIpEndPoint;
            IPEndPoint senderIpEndPoint;
            UdpClient socket;

            public UdpListener(string hostname, int port) 
            {
                lastReceivedBytes = new byte[1024];
                localIpEndPoint = new IPEndPoint(IPAddress.Parse(hostname), port);
                socket = new UdpClient(localIpEndPoint);
                socket.Client.ReceiveTimeout = 5000;
                senderIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }

            // Sets lastReceivedBytes with the last message received, or null if it
            // times out. This call is blocking; you may want to run it in a
            // thread while you send the message.
            public void Listen()
            {
                try
                {
                    lastReceivedBytes = socket.Receive(ref senderIpEndPoint);
                }
                catch (SocketException ex)
                {
                    // If we timeout, just set the last received message to null (which
                    // will behave as expected in assertions).
                    // If we get another error, propagate it upwards 
                    if (ex.ErrorCode == 10060) // WSAETIMEDOUT; Timeout error
                        lastReceivedBytes = null;
                    else
                        throw;
                }
            }

            // Encode lastReceivedBytes as a string, and clear them before
            // returning the string. This allows us to use the same UdpListener instance
            // for every test we we never have to worry about a message received from a
            // previous test giving us a false positive.
            public string GetAndClearLastMessage()
            {
                if (lastReceivedBytes == null)
                    return null;

                string encodedMessage = Encoding.ASCII.GetString(lastReceivedBytes, 0,
                                                                 lastReceivedBytes.Length);
                lastReceivedBytes = null;
                return encodedMessage;

            }

            public void Dispose() 
            {
                socket.Close();
            }
        }
    }
}
