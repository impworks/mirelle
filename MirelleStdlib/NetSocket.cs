using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MirelleStdlib
{
  public enum NetState
  {
    Default,
    Ready,
    Closed
  }

  public class NetSocket
  {
    private Socket Socket;
    private Socket BaseSocket;
    private NetState State;

    public NetSocket()
    {
      State = NetState.Default;
    }

    /// <summary>
    /// Ensure socket is in operable mode
    /// </summary>
    /// <param name="startup">Allow startup or not</param>
    private void Validate(bool startup = false)
    {
      if (!startup && State == NetState.Default)
        throw new Exception("Socket is not connected!.");

      if(State == NetState.Closed)
        throw new Exception("Socket cannot be reused after it has been closed!");
    }

    /// <summary>
    /// Listen to the network until some data is received
    /// </summary>
    public void Bind(int port)
    {
      // ensure old connection removed
      if (Socket != null)
        Socket.Close();

      // create a waiting socket at current port
      BaseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      var ipe = new IPEndPoint(IPAddress.Any, port);
      BaseSocket.Bind(ipe);
      State = NetState.Ready;
    }

    /// <summary>
    /// Listen to a specific port
    /// </summary>
    public void Listen()
    {
      BaseSocket.Listen(0);
      Socket = BaseSocket.Accept();
    }

    /// <summary>
    /// Connect to a remote place
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip, int port)
    {
      if (Socket != null)
        Socket.Close();

      // create a waiting socket at current port
      Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      Socket.Connect(IPAddress.Parse(ip), port);
      State = NetState.Ready;
    }

    /// <summary>
    /// Check whether data is available
    /// </summary>
    /// <returns></returns>
    public bool CanRead()
    {
      return State == NetState.Ready && Socket.Available > 0;
    }

    /// <summary>
    /// Check whether socket is connected
    /// </summary>
    /// <returns></returns>
    public bool CanWrite()
    {
      return State == NetState.Ready;
    }

    /// <summary>
    /// Read the data from socket as an array of ints
    /// </summary>
    /// <returns></returns>
    public int[] ReadBytes()
    {
      Validate();

      var len = Socket.Available;
      if (len == 0) return null;

      // create buffers
      var arr = new int[len];
      var bytes = new byte[len];

      // receive data from socket
      Socket.Receive(bytes);

      // convert to ints
      for (int idx = 0; idx < len; idx++)
        arr[idx] = bytes[idx];

      return arr;
    }

    /// <summary>
    /// Read data from socket as a string
    /// </summary>
    /// <returns></returns>
    public string Read()
    {
      Validate();

      var len = Socket.Available;
      if (len == 0) return null;

      // retrieve data into buffer
      var bytes = new byte[len];
      Socket.Receive(bytes);

      return System.Text.Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Write an array of bytes
    /// </summary>
    /// <param name="arr"></param>
    public void WriteBytes(int[] arr)
    {
      Validate();

      var bytes = new byte[arr.Length];
      for (int idx = 0; idx < arr.Length; idx++)
        bytes[idx] = (byte)arr[idx];

      Socket.Send(bytes);
    }

    /// <summary>
    /// Write a string
    /// </summary>
    /// <param name="str"></param>
    public void Write(string str)
    {
      Validate();

      Socket.Send(System.Text.Encoding.UTF8.GetBytes(str));
    }

    /// <summary>
    /// Refresh the socket
    /// </summary>
    public void Refresh()
    {
      Socket.Disconnect(false);
    }

    /// <summary>
    /// Close the socket so that no further work is possible
    /// </summary>
    public void Close()
    {
      Socket.Close();
      Socket = null;
      State = NetState.Closed;
    }
  }
}
