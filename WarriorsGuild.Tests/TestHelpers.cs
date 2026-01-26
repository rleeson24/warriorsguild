using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using WarriorsGuild.Data.Models;

namespace WarriorsGuild.Tests
{
    static class TestHelpers
    {
        //public static IQueryable<T> Include<T, TProperty>( this IQueryable<T> sequence, Expression<Func<T, TProperty>> path ) where T : class
        //{
        //	var dbQuery = sequence as DbQuery<T>;
        //	if (dbQuery != null)
        //	{
        //		return dbQuery.Include( path );
        //	}
        //	return sequence;
        //}
        public static Mock<DbSet<T>> CreateDbSetMock<T>( List<T> elements ) where T : class
        {
            var elementsAsQueryable = elements.AsQueryable();
            var MockSet = new Mock<DbSet<T>>( MockBehavior.Loose );
            //MockSet.As<IAsyncEnumerable<T>>().Setup( x => x.GetAsyncEnumerator( default ) ).Returns( new TestAsyncEnumerator<T>( elementsAsQueryable.GetEnumerator() ) );
            MockSet.As<IQueryable<T>>().Setup( x => x.Provider ).Returns( new TestAsyncQueryProvider<T>( elementsAsQueryable.Provider ) );
            MockSet.As<IQueryable<T>>().Setup( x => x.Expression ).Returns( elementsAsQueryable.Expression );
            //MockSet.As<IQueryable<T>>().Setup( x => x.ElementType ).Returns( elementsAsQueryable.ElementType );
            MockSet.As<IQueryable<T>>().Setup( x => x.GetEnumerator() ).Returns( elementsAsQueryable.GetEnumerator() );
            //MockSet.As<IEnumerable<T>>().Setup( x => x.GetEnumerator() ).Returns( elementsAsQueryable.GetEnumerator() );
            return MockSet;
        }

        public static Mock<DbSet<T>> CreateDbSetMock<T>( IEnumerable<T> elements ) where T : class
        {
            return CreateDbSetMock<T>( elements, false, false );
        }

        public static Mock<DbSet<T>> CreateDbSetMock<T>( IEnumerable<T> elements, Boolean includeEnumerator, Boolean excludeExpression ) where T : class
        {
            var elementsAsQueryable = elements.AsQueryable();
            var MockSet = new Mock<DbSet<T>>( MockBehavior.Loose );
            //MockSet.As<IAsyncEnumerable<T>>().Setup( x => x.GetAsyncEnumerator( default ) ).Returns( new TestAsyncEnumerator<T>( elementsAsQueryable.GetEnumerator() ) );
            if ( !excludeExpression )
            {
                MockSet.As<IQueryable<T>>().Setup( x => x.Provider ).Returns( new TestAsyncQueryProvider<T>( elementsAsQueryable.Provider ) );
                MockSet.As<IQueryable<T>>().Setup( x => x.Expression ).Returns( elementsAsQueryable.Expression );
            }
            //MockSet.As<IQueryable<T>>().Setup( x => x.ElementType ).Returns( elementsAsQueryable.ElementType );
            if ( includeEnumerator )
            {
                MockSet.As<IQueryable<T>>().Setup( x => x.GetEnumerator() ).Returns( elementsAsQueryable.GetEnumerator() );
                MockSet.As<IEnumerable<T>>().Setup( x => x.GetEnumerator() ).Returns( elementsAsQueryable.GetEnumerator() );
            }
            return MockSet;
        }

        // Async query provider for unit testing
        internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider( IQueryProvider inner )
            {
                _inner = inner;
            }

            public IQueryable CreateQuery( Expression expression )
            {
                return new TestAsyncEnumerable<TEntity>( expression );
            }

            public IQueryable<TElement> CreateQuery<TElement>( Expression expression )
            {
                return new TestAsyncEnumerable<TElement>( expression );
            }

            public object Execute( Expression expression )
            {
                return _inner.Execute( expression );
            }

            public TResult Execute<TResult>( Expression expression )
            {
                return _inner.Execute<TResult>( expression );
            }

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>( Expression expression )
            {
                return new TestAsyncEnumerable<TResult>( expression );
            }

            //
            // Approach from MockQueryable by romantitov https://github.com/romantitov
            // (https://github.com/romantitov/MockQueryable/blob/master/src/MockQueryable/MockQueryable/TestAsyncEnumerable.cs#L59-L73)
            //
            // (Thanks https://github.com/SuricateCan for your suggestion!)
            //
            public TResult ExecuteAsync<TResult>( Expression expression, CancellationToken cancellationToken )
            {
                var expectedResultType = typeof( TResult ).GetGenericArguments()[ 0 ];
                var executionResult = typeof( IQueryProvider )
                    .GetMethod(
                        name: nameof( IQueryProvider.Execute ),
                        genericParameterCount: 1,
                        types: new[] { typeof( Expression ) } )
                    .MakeGenericMethod( expectedResultType )
                    .Invoke( this, new[] { expression } );

                return (TResult)typeof( Task ).GetMethod( nameof( Task.FromResult ) )
                    .MakeGenericMethod( expectedResultType )
                    .Invoke( null, new[] { executionResult } );
            }
        }

        // Async enumerable for unit testing
        internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable( IEnumerable<T> enumerable )
                : base( enumerable )
            { }

            public TestAsyncEnumerable( Expression expression )
                : base( expression )
            { }

            public IAsyncEnumerator<T> GetAsyncEnumerator( CancellationToken cancellationToken )
            {
                return GetEnumerator();
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return new TestAsyncEnumerator<T>( this.AsEnumerable().GetEnumerator() );
            }

            IQueryProvider IQueryable.Provider
            {
                get { return new TestAsyncQueryProvider<T>( this ); }
            }
        }

        // Async enumerator for unit testing
        internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator( IEnumerator<T> inner )
            {
                _inner = inner;
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }

            public T Current
            {
                get
                {
                    return _inner.Current;
                }
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>( _inner.MoveNext() );
            }
        }

        public class FakeUserManager : UserManager<ApplicationUser>
        {
            //private IQueryable<ApplicationUser> users;

            public FakeUserManager( IQueryableUserStore<ApplicationUser> userStore )
                : base( userStore,
                      new Mock<IOptions<IdentityOptions>>().Object,
                      new Mock<IPasswordHasher<ApplicationUser>>().Object,
                      new IUserValidator<ApplicationUser>[ 0 ],
                      new IPasswordValidator<ApplicationUser>[ 0 ],
                      new Mock<ILookupNormalizer>().Object,
                      new Mock<IdentityErrorDescriber>().Object,
                      new Mock<IServiceProvider>().Object,
                      new Mock<ILogger<UserManager<ApplicationUser>>>().Object )
            { }

            //public FakeUserManager( IQueryable<ApplicationUser> users ) : this()
            //{
            //    this.Users = users;
            //}

            public override Task<IdentityResult> CreateAsync( ApplicationUser user, string password )
            {
                return Task.FromResult( IdentityResult.Success );
            }
        }

        public class FakeSignInManager : SignInManager<ApplicationUser>
        {
            public FakeSignInManager( IHttpContextAccessor contextAccessor, IQueryableUserStore<ApplicationUser> userStore )
                : base( new FakeUserManager( userStore ),
                      contextAccessor,
                      new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                      new Mock<IOptions<IdentityOptions>>().Object,
                      new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
                      new Mock<IAuthenticationSchemeProvider>().Object,
                      new Mock<IUserConfirmation<ApplicationUser>>().Object )
            {
            }

            public override Task SignInAsync( ApplicationUser user, bool isPersistent, string authenticationMethod = null )
            {
                return Task.FromResult( 0 );
            }

            public override Task<SignInResult> PasswordSignInAsync( string userName, string password, bool isPersistent, bool lockoutOnFailure )
            {
                return Task.FromResult( SignInResult.Success );
            }

            public override Task SignOutAsync()
            {
                return Task.FromResult( 0 );
            }
        }
    }
}