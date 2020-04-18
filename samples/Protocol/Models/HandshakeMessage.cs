using System;
using System.Text;

namespace Protocol.Models
{
    public class HandshakeMessage : IMessage
    {
        public string Message { get; set; }

        public const int Id = 1;
        public int MessageId => Id;

        public int GetLength() => !string.IsNullOrWhiteSpace(Message)
            ? Encoding.UTF8.GetBytes(Message).Length
            : 0;
    }
}
