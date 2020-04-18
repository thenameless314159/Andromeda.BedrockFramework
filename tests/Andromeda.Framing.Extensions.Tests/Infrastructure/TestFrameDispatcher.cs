using System;
using System.Collections.Generic;
using System.Text;
using Andromeda.Framing.Behaviors;
using Andromeda.Framing.Protocol;

namespace Andromeda.Framing.Extensions.Tests.Infrastructure
{
    internal class TestFrameDispatcher : FrameDispatcher
    {
        public IEnumerable<int> Mappings => _handlers.Keys;

        public TestFrameDispatcher(IServiceProvider sp, IMessageReader decoder) : base(sp, decoder)
        {
        }
    }
}
