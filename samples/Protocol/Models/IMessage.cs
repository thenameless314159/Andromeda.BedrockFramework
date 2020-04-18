using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Models
{
    public interface IMessage
    {
        int MessageId { get; }
        int GetLength();
    }
}
