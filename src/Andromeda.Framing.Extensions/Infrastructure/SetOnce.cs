using System;
using System.Threading;

namespace Andromeda.Framing.Extensions.Infrastructure
{
    internal sealed class SetOnce<T> where T : class
    {
        private readonly bool _shouldThrow;
        private readonly string _nameOf;
        private T _value;

        public T Value => _value;

        public SetOnce(bool shouldThrow = true, string propertyName = default)
        {
            _shouldThrow = shouldThrow;
            _nameOf = propertyName;
        }

        public void Set(in T value)
        {
            var oldValue = Interlocked.CompareExchange(ref _value, value, null);

            if (oldValue == null) return;
            if (_shouldThrow) throw new InvalidOperationException(
                $"{_nameOf ?? typeof(T).Name} was already set and was registered with once behavior !");
        }

        public static implicit operator T(SetOnce<T> once) => once.Value;

        public override string ToString() => _value?.ToString() ?? string.Empty;
    }
}
