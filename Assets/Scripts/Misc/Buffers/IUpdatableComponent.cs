namespace MajdataPlay.Buffers
{
    public interface IUpdatableComponent<TState> : IStateful<TState>
    {
        bool Active { get; }
        void ComponentUpdate();
    }
}
