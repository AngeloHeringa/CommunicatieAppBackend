using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }

    public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var resultType = typeof(TResult).GetTypeInfo();
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerResultType = resultType.GetGenericArguments()[0];
            var executeMethod = typeof(TestAsyncQueryProvider<TEntity>)
                .GetMethod(nameof(ExecuteAsyncGeneric), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(innerResultType);
            return (Task<TResult>)executeMethod.Invoke(this, new object[] { expression, cancellationToken });
        }
        else
        {
            return Task.FromResult(Execute<TResult>(expression));
        }
    }

    public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
    {
        return Task.FromResult(_inner.Execute(expression));
    }

    TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }

    private async Task<TResult> ExecuteAsyncGeneric<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var result = Execute<TResult>(expression);
        return await Task.FromResult(result);
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetEnumerator()
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider
    {
        get { return new TestAsyncQueryProvider<T>(this); }
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public void Dispose()
    {
        _inner.Dispose();
    }

    public T Current
    {
        get
        {
            return _inner.Current;
        }
    }

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        return Task.FromResult(_inner.MoveNext());
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string? ToString()
    {
        return base.ToString();
    }

}
