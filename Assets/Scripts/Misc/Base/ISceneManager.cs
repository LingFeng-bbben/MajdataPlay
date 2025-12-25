using System.Threading;

public interface ISceneManager
{
    CancellationToken CancellationToken { get; }
}