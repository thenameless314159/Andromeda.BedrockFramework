using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Models
{
    public class HelloServerMessage : IMessage
    {
        public string Response { get; set; }

        public const int Id = 2;
        public int MessageId => Id;

        public int GetLength() => !string.IsNullOrWhiteSpace(Response)
            ? Encoding.UTF8.GetBytes(Response).Length
            : 0;
    }
}
