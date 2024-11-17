
namespace MajdataPlay.Interfaces
{
    public interface IFixedUpdatableComponent<TState> : IStateful<TState>
    {
        void ComponentFixedUpdate();
    }
}
