using System;
using System.Threading.Tasks;

namespace StatsdClient
{
    public interface IStatsdClient : IDisposable
    {
        void Send(string command);
        Task SendAsync(string command);
    }
}
#if NET45
namespace System.Net.Sockets
{
    public static class SocketTaskExtensions
	{
		public static Task ConnectAsync(this Socket socket, EndPoint remoteEP)
		{
			return Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, remoteEP, null);
		}

		public static Task<Int32> SendAsync(this Socket socket, ArraySegment<byte> buffer, SocketFlags socketFlags)
		{
			return Task.Factory.FromAsync((arg1, arg2, callback, state) => socket.BeginSend(arg1.Array, arg1.Offset, arg1.Count, arg2, callback, state),
                socket.EndSend, buffer, socketFlags, null);
		}

		public static Task<Int32> SendToAsync(this Socket socket, ArraySegment<byte> buffer, SocketFlags socketFlags, EndPoint remoteEP)
		{
			return Task.Factory.FromAsync((arg1, arg2, arg3, callback, state) => socket.BeginSendTo(arg1.Array, arg1.Offset, arg1.Count, arg2, arg3, callback, state),
                socket.EndSendTo, buffer, socketFlags, remoteEP, null);
		}
	}
}
#endif