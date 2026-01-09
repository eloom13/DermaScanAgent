namespace AiAgents.Core.Abstractions;

public interface IPercept { }
public interface IAction { }
public interface IResult { }

public interface IPerceptionSource<TPercept> where TPercept : IPercept
{
    Task<TPercept?> PerceiveAsync(CancellationToken ct);
}

public abstract class SoftwareAgent<TPercept, TAction, TResult>
    where TPercept : IPercept
    where TAction : IAction
    where TResult : IResult
{
    public abstract Task<TResult?> StepAsync(CancellationToken ct);
}