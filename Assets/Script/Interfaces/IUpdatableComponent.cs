
namespace MajdataPlay.Interfaces
{
    public interface IUpdatableComponent<TState> : IStateful<TState>
    {
        void ComponentUpdate();
    }
}
