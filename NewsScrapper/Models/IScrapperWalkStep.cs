namespace NewsScrapper.Models;

public interface IScrapperWalkStep
{
    public void Step()
    {
    }
}

public interface IScrapperWalkStepStart<T> : IScrapperWalkStep
{
    public new T Step();
}

public interface IScrapperIntermediateWalkStep<TIn, TOut> : IScrapperWalkStep
{
    public TOut Step(TIn entries);
}

public interface IScrapperWalkStepEnd<T> : IScrapperWalkStep
{
    public void Step(T arg);
}