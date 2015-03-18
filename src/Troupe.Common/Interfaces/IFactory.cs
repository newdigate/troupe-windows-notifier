namespace Troupe.Common.Interfaces {
    public interface IFactory<out T> {
        T Create();
    }
}