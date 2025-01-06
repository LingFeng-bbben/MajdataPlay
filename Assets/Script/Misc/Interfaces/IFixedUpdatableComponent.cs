
namespace MajdataPlay.Interfaces
{
    public interface IFixedUpdatableComponent<TState> : IStateful<TState>
    {
        bool Active { get; }
        void ComponentFixedUpdate();
    }
}
