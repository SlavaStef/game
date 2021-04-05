using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PokerHand.Common.Entities;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.DataAccess.Context;
using PokerHand.DataAccess.Repositories;
using Xunit;

namespace PokerHand.DataAccess.Tests.Repositories
{
    public class ExternalLoginRepositoryTests
    {
        private readonly Mock<ApplicationContext> _contextMock = new Mock<ApplicationContext>();

        private DbSet<T> CreateDbSet<T>(IQueryable<T> collection) where T:class
        {
            var stubDbSet = new Mock<DbSet<T>>();
            stubDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(collection.Provider);
            stubDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(collection.Expression);
            stubDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(collection.ElementType);
            stubDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(collection.GetEnumerator());
            return stubDbSet.Object;
        }
        
        [Fact]
        public async Task GetByProviderKey_ReturnsPlayerId_IfProviderKeyExistsInDatabase()
        {
            var playerId = Guid.NewGuid();
            const string providerKey = "12345";

            var logins = new List<ExternalLogin>
            {
                new() {ProviderName = ExternalProviderName.Google, PlayerId = playerId, ProviderKey = providerKey}
            };
            
            var dbSet = CreateDbSet(logins.AsQueryable());

            _contextMock
                .Setup(o => o.ExternalLogins)
                .Returns(dbSet);

            var sut = new ExternalLoginRepository(_contextMock.Object);

            var result = await sut.GetByProviderKey(providerKey);

            result.Should().Be(playerId);
        }
        
        [Fact]
        public async Task GetByProviderKey_ReturnsEmptyGuid_IfProviderKeyDoesntExistInDatabase()
        {
            var playerId = Guid.NewGuid();
            const string googleProviderKey = "12345";

            var logins = new List<ExternalLogin>
            {
                new() {ProviderName = ExternalProviderName.Google, PlayerId = playerId, ProviderKey = googleProviderKey}
            };
            
            var dbSet = CreateDbSet(logins.AsQueryable());

            _contextMock
                .Setup(o => o.ExternalLogins)
                .Returns(dbSet);

            var sut = new ExternalLoginRepository(_contextMock.Object);

            var result = await sut.GetByProviderKey("6789");

            result.Should().Be(Guid.Empty);
        }
    }
}