namespace Andromeda.Framing.Extensions.Infrastructure
{
    internal static class BoxingOperations
    {
        public static Box<T> Box<T>(this T value) where T : struct => new Box<T>(value);
    }

    internal sealed class Box<T>
    {
        public T Value { get; set; }
        public Box(in T value) => Value = value;
        public static implicit operator T(Box<T> box) => box.Value;
    }
}
