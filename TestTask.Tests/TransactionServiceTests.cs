using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TestTask.Application.Interfaces;
using TestTask.Application.Services;
using TestTask.Application.Strategies;
using TestTask.Core;
using TestTask.Core.Entities;
using TestTask.Core.Exceptions;
using TestTask.Infrastructure.Data;
using Xunit;


   public class TransactionServiceTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TransactionService _service;
    private readonly Mock<IClientSpecificRepository> _mockClientRepository;
    private readonly Guid _clientId = Guid.NewGuid();

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var strategies = new List<ITransactionStrategy>
        {
            new CreditStrategy(),
            new DebitStrategy()
        };

        _mockClientRepository = new Mock<IClientSpecificRepository>();
        _service = new TransactionService(_dbContext, _mockClientRepository.Object, strategies);
    }

    private Mock<ITransaction> CreateTxMock(Guid id, decimal amount)
    {
        var mock = new Mock<ITransaction>();
        mock.Setup(t => t.Id).Returns(id);
        mock.Setup(t => t.ClientId).Returns(_clientId);
        mock.Setup(t => t.Amount).Returns(amount);
        mock.Setup(t => t.DateTime).Returns(DateTime.UtcNow);
        return mock;
    }

    [Fact]
    public async Task CreditAsync_ShouldIncreaseBalance_AndCreateRecords()
    {
        var client = new Client { Id = _clientId, Name = "Test", Balance = 100m };
        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync();
        
        _mockClientRepository.Setup(r => r.GetClientForUpdateAsync(_clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var txId = Guid.NewGuid();
        var tx = CreateTxMock(txId, 50m).Object;

        var response = await _service.CreditAsync(tx, CancellationToken.None);

        client.Balance.Should().Be(150m);
        response.ClientBalance.Should().Be(150m);

        var history = await _dbContext.TransactionHistory.FirstOrDefaultAsync(h => h.FinanceTransactionId == txId);
        history.Should().NotBeNull();
        history.Status.Should().Be(TransactionStatus.Completed);
    }

    [Fact]
    public async Task DebitAsync_ShouldDecreaseBalance_WhenFundsEnough()
    {
        var client = new Client { Id = _clientId, Name = "Test", Balance = 100m };
        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync();
        
        _mockClientRepository.Setup(r => r.GetClientForUpdateAsync(_clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var tx = CreateTxMock(Guid.NewGuid(), 40m).Object;

        await _service.DebitAsync(tx, CancellationToken.None);

        client.Balance.Should().Be(60m);
    }

    [Fact]
    public async Task DebitAsync_ShouldThrowException_WhenInsufficientFunds()
    {
        var client = new Client { Id = _clientId, Name = "Test", Balance = 10m };
        
        _mockClientRepository.Setup(r => r.GetClientForUpdateAsync(_clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var tx = CreateTxMock(Guid.NewGuid(), 50m).Object;

        var act = async () => await _service.DebitAsync(tx, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Недостаточно средств");
    }

    [Fact]
    public async Task CreditAsync_IsIdempotent()
    {
        var client = new Client { Id = _clientId, Name = "Test", Balance = 100m };
        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync();
        
        _mockClientRepository.Setup(r => r.GetClientForUpdateAsync(_clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var txId = Guid.NewGuid();
        var tx = CreateTxMock(txId, 50m).Object;

        await _service.CreditAsync(tx, CancellationToken.None); 
        var secondResponse = await _service.CreditAsync(tx, CancellationToken.None); 

        client.Balance.Should().Be(150m); 
        _dbContext.TransactionHistory.Count(h => h.FinanceTransactionId == txId).Should().Be(1);
    }

    [Fact]
    public async Task RevertAsync_ShouldRestoreBalance_ForCredit()
    {
        var txId = Guid.NewGuid();
        var client = new Client { Id = _clientId, Name = "Test", Balance = 150m };
        _dbContext.Clients.Add(client);

        var tx = new FinanceTransaction
        {
            Id = txId,
            ClientId = _clientId,
            Amount = 50m,
            TransactionType = TransactionType.Credit
        };
        _dbContext.FinanceTransaction.Add(tx);
        await _dbContext.SaveChangesAsync();
        
        _mockClientRepository.Setup(r => r.GetClientForUpdateAsync(_clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        await _service.RevertAsync(txId, CancellationToken.None);

        client.Balance.Should().Be(100m);
        var history = await _dbContext.TransactionHistory
            .FirstOrDefaultAsync(h => h.FinanceTransactionId == txId && h.Status == TransactionStatus.Reverted);
        history.Should().NotBeNull();
    }

    [Fact]
    public async Task RevertAsync_ShouldBeIdempotent()
    {
        var txId = Guid.NewGuid();
        var client = new Client { Id = _clientId, Name = "Test", Balance = 150m };
        _dbContext.Clients.Add(client);

        var tx = new FinanceTransaction { Id = txId, ClientId = _clientId, Amount = 50m, TransactionType = TransactionType.Credit };
        _dbContext.FinanceTransaction.Add(tx);
        await _dbContext.SaveChangesAsync();
        
        _mockClientRepository.Setup(r => r.GetClientForUpdateAsync(_clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        await _service.RevertAsync(txId, CancellationToken.None);
        await _service.RevertAsync(txId, CancellationToken.None); 

        client.Balance.Should().Be(100m);
        _dbContext.TransactionHistory.Count(h => h.FinanceTransactionId == txId && h.Status == TransactionStatus.Reverted)
            .Should().Be(1);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnCorrectBalance()
    {
        var client = new Client { Id = _clientId, Name = "Test", Balance = 123.45m };
        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync();

        var response = await _service.GetBalanceAsync(_clientId, CancellationToken.None);

        response.ClientBalance.Should().Be(123.45m);
    }
}