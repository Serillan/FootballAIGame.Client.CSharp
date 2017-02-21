﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Text;
using FootballAIGame.AI.FSM.SimulationEntities;

namespace FootballAIGame.AI.FSM
{
    /// <summary>
    /// Responsible for keeping TCP connection to the game server.
    /// Provides methods for communicating with the server.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    class ServerConnection : IDisposable
    {
        /// <summary>
        /// Gets or sets the TCP client associated with the game server.
        /// </summary>
        /// <value>
        /// The TCP client connected to the game server.
        /// </value>
        private TcpClient ServerTcpClient { get; set; }

        /// <summary>
        /// Gets or sets the network stream associated with the game server.
        /// </summary>
        /// <value>
        /// The network stream.
        /// </value>
        private NetworkStream NetworkStream { get; set; }

        /// <summary>
        /// Connects to the game server.
        /// </summary>
        /// <param name="address">The game server IP address.</param>
        /// <param name="port">The game server port.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="aiName">Desired name of the AI.</param>
        /// <returns></returns>
        /// <exception cref="ServerException">
        /// Thrown if an error has occurred while connecting to the server.
        /// </exception>
        public static ServerConnection Connect(IPAddress address, int port,
            string userName, string aiName)
        {
            var connection = new ServerConnection { ServerTcpClient = new TcpClient { NoDelay = true } };

            try
            {
                var connect = connection.ServerTcpClient.BeginConnect(address, port, null, null);
                var waitHandle = connect.AsyncWaitHandle;
                var succesfull = waitHandle.WaitOne(5000);
                connection.ServerTcpClient.EndConnect(connect);

                if (!succesfull || !connection.ServerTcpClient.Connected)
                    throw new ServerException("Server is not responding.");
            }
            catch (SocketException)
            {
                throw new ServerException("Server is not responding.");
            }

            connection.ServerTcpClient.NoDelay = true;
            connection.NetworkStream = connection.ServerTcpClient.GetStream();
            connection.Send(string.Format("LOGIN {0} {1}", userName, aiName));

            var message = connection.ReceiveMessage();

            if (message == "CONNECTED")
                return connection;
            else
            {
                connection.Dispose();
                throw new ServerException(message);
            }
        }

        /// <summary>
        /// Receives the server <see cref="Command"/>.
        /// </summary>
        /// <returns>The server <see cref="Command"/></returns>
        public Command ReceiveCommand()
        {
            while (true)
            {
                var firstLine = ReadLine();
                if (firstLine.Length >= 14 && firstLine.Substring(firstLine.Length - 14) == "GET PARAMETERS")
                    return new Command()
                    {
                        Type = CommandType.GetParameters,
                    };
                else if (firstLine.Length >= 10 && firstLine.Substring(firstLine.Length - 10) == "GET ACTION")
                {
                    //Console.WriteLine("reading state data");
                    var data = new byte[372];
                    NetworkStream.Read(data, 0, data.Length);

                    return new Command()
                    {
                        Type = CommandType.GetAction,
                        Data = data
                    };
                }
                else if (firstLine != "keepalive")
                    Console.WriteLine(firstLine);

            }
        }

        /// <summary>
        /// Receives the string message.
        /// </summary>
        /// <returns>The string message.</returns>
        public string ReceiveMessage()
        {
            var message = ReadLine();
            return message;
        }

        /// <summary>
        /// Reads the next line.
        /// </summary>
        /// <returns>The string read from the line.</returns>
        public string ReadLine()
        {
            var bytes = new List<byte>();
            var buffer = new byte[1];

            while (true)
            {
                buffer[0] = (byte)NetworkStream.ReadByte();
                //var asyncReader = NetworkStream.BeginRead(buffer, 0, 1, null, null);
                //var waitHandle = asyncReader.AsyncWaitHandle;
                //waitHandle.WaitOne();
                //NetworkStream.EndRead(asyncReader);

                if (buffer[0] == (int)'\n')
                    break;
                bytes.Add(buffer[0]);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Sends the specified game action to the game server.
        /// </summary>
        /// <param name="action">The action.</param>
        public void Send(GameAction action)
        {
            var data = new float[44];
            for (var i = 0; i < 11; i++)
            {
                data[4*i] = (float)action.PlayerActions[i].Movement.X;
                data[4 * i + 1] = (float)action.PlayerActions[i].Movement.Y;
                data[4 * i + 2] = (float)action.PlayerActions[i].Kick.X;
                data[4 * i + 3] = (float)action.PlayerActions[i].Kick.Y;
            }

            var byteArray = new byte[data.Length * 4 + 4];
            var numArray = new int[1] { action.Step };

            Buffer.BlockCopy(numArray, 0, byteArray, 0, 4);
            Buffer.BlockCopy(data, 0, byteArray, 4, data.Length * 4);

            //Console.WriteLine("sending action");
            Send("ACTION");
            Send(byteArray);
            //Console.WriteLine("action sent");
        }

        /// <summary>
        /// Sends the players parameters to the game server.
        /// </summary>
        /// <param name="players">The players with their parameters set.</param>
        public void SendParameters(FootballPlayer[] players)
        {
            var data = new float[44];
            for (int i = 0; i < 11; i++)
            {
                data[4 * i] = players[i].Speed;
                data[4 * i + 1] = players[i].Precision;
                data[4 * i + 2] = players[i].Possession;
                data[4 * i + 3] = players[i].KickPower;
            }

            var byteArray = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);

            Send("PARAMETERS");
            Send(byteArray);
        }

        /// <summary>
        /// Sends the specified string message to the game server.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message + "\n");
            Send(bytes);
        }

        /// <summary>
        /// Sends the specified data to the game server.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Send(byte[] data)
        {
            NetworkStream.Write(data, 0, data.Length);
            NetworkStream.Flush();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ServerTcpClient.Close();
        }
    }
}