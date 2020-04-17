using System;
using System.Threading;

namespace Andromeda.Framing.Extensions.Infrastructure
{
    internal sealed class WriteOnce<T>
    {
        private readonly bool _shouldThrow;
        private readonly string _nameOf;
        private Box<T> _box;

        public T Value => _box == null ? default : _box.Value;

        public WriteOnce(bool shouldThrow = true, string propertyName = default)
        {
            _shouldThrow = shouldThrow;
            _nameOf = propertyName;
        }

        public void Set(in T value)
        {
            var oldValue = Interlocked.CompareExchange(ref _box, new Box<T>(in value), null);

            if (oldValue == null) return;
            if (_shouldThrow) throw new InvalidOperationException(
                $"{_nameOf ?? typeof(T).Name} was already set and was registered with once behavior !");
        }

        public static implicit operator T(WriteOnce<T> once) => once.Value;
        public override string ToString() => _box?.Value?.ToString() ?? string.Empty;
    }
}
