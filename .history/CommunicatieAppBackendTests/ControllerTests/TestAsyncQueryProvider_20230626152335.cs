using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

public class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncQueryable<T>(this, expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncQueryable<TElement>(this, expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var innerResult = _inner.Execute<IEnumerable<TResult>>(expression);
        return Task.FromResult(innerResult).Result;
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        // var innerResult = _inner.Execute<IEnumerable<TResult>>(expression);
        // return new TestAsyncEnumerable<TResult>(innerResult);

    }

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
        throw new System.NotImplementedException();
    }

    TResult IQueryProvider.Execute<TResult>(Expression expression)
    {
        throw new System.NotImplementedException();
    }
}
