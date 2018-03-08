﻿using System;
using System.IO;
using System.Net.Sockets;

namespace Shared
{
    public abstract class GlobalNetworkManager
    {
        const int DEFAULT_BUFFER_SIZE = 4046;

        public NetworkStream networkStream { get; set; }

        public StreamWriter streamWriter { get; set; }

        public StreamReader streamReader { get; set; }

        public BinaryWriter binaryWriter { get; set; }

        public BinaryReader binaryReader { get; set; }

        #region Events

        public delegate void TransfertStartHandler(long total);

        public event TransfertStartHandler StreamTransfertStartEvent;

        public delegate void TransfertProgressHandler(long current);

        public event TransfertProgressHandler StreamTransfertProgressEvent;

        public delegate void TransfertFailHandler();

        public event TransfertFailHandler StreamTransfertFailEvent;

        #endregion Event


        /// <summary>
        /// Read one line from the network stream
        /// </summary>
        /// <returns>String read from stream</returns>
        public string ReadLine()
        {
            try
            {
                var data = streamReader.ReadLine();
                if (data == null)
                {
                    throw new NetworkException();
                }
                return data;
            }
            catch (Exception)
            {
                throw new NetworkException();
            }
        }


        /// <summary>
        /// Read all the given ReadStream content and write it to the network stream as byte arrays, flushing each time
        /// </summary>
        /// <param name="stream">Stream to process, must be a readable stream</param>
        public void StreamToNetworkStream(Stream stream)
        {
            if (!stream.CanRead)
            {
                // The stream can't be read : invalid argument
                throw new ArgumentException();
            }

            // Wait for ready flag
            ReadLine();

            var buffer = new byte[DEFAULT_BUFFER_SIZE];
            int bytesRead;
            long bytesWritten = 0;

            // Notify that the stream transfert started if the handler has been implemented
            StreamTransfertStartEvent?.Invoke(stream.Length);

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                try
                {
                    binaryWriter.Write(buffer, 0, bytesRead);
                    binaryWriter.Flush();
                    bytesWritten += bytesRead;

                    // Notify that the stream transfert progress changed if the handler has been implemented
                    StreamTransfertProgressEvent?.Invoke(bytesWritten);
                }
                catch (Exception)
                {
                    StreamTransfertFailEvent?.Invoke();
                    throw new NetworkException();
                }
            }
        }


        /// <summary>
        /// Read all the network ReadStream content and write it to the given stream
        /// </summary>
        /// <param name="stream">Stream to process, must be a writable stream</param>
        /// <param name="dataSize">Size of the data to write</param>
        public void NetworkStreamToStream(Stream stream, long dataSize)
        {
            if (!stream.CanWrite)
            {
                // The stream can't be written : invalid argument
                throw new ArgumentException();
            }

            // Send ready flag
            WriteLine("OK");

            var buffer = new byte[DEFAULT_BUFFER_SIZE];
            long bytesWritten = 0;
            try
            {
                // Notify that the stream transfert started if the handler has been implemented
                StreamTransfertStartEvent?.Invoke(dataSize);

                int bytesRead;
                while ((bytesRead = binaryReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                    bytesWritten += bytesRead;

                    // Notify that the stream transfert progress changed if the handler has been implemented
                    StreamTransfertProgressEvent?.Invoke(bytesWritten);

                    // The file has been totally written
                    if (bytesWritten == dataSize)
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                StreamTransfertFailEvent?.Invoke();
                throw new NetworkException();
            }
        }


        /// <summary>
        /// Write the given string to the network stream, then flush
        /// </summary>
        /// <param name="data">String to write</param>
        public void WriteLine(string data)
        {
            try
            {
                streamWriter.WriteLine(data);
                streamWriter.Flush();
            }
            catch (Exception)
            {
                throw new NetworkException();
            }
        }


        /// <summary>
        /// Close the network stream
        /// </summary>
        public void Cleanup() => networkStream.Close();
    }
}