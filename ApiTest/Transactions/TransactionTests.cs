using System.Security.Claims;
using ApiTest.Mocks;
using ExpenseReconciliation.Controllers;
using ExpenseReconciliation.DataContext;
using ExpenseReconciliation.Domain.Models;
using ExpenseReconciliation.Repository;
using ExpenseReconciliation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiTest.Transactions;

public class TransactionTests
{
    private TransactionController _transactionController;

    /**
     * I don't really know the test data annotations and don't have time to learn, so a lot is just done the manual way
     */
    [SetUp]
    public async Task Setup()
    {
        var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "dev_expenses")
            .Options;
        var appDbContext = new AppDbContext(dbContextOptions);
        var auditServiceMock = new Mock<IAuditService>();
        var accountServiceMock = new Mock<IAccountService>();
        accountServiceMock.Setup(
            s => s.FindOrCreateId(It.IsAny<string>())).ReturnsAsync(1);
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(
            s => s.GetUserByEmail(It.IsAny<string>())).ReturnsAsync(new User());

        var transactionRepository = new TransactionRepository(appDbContext, Mock.Of<ILogger<TransactionRepository>>());
        var importRecordRepository = new ImportRecordRepository(appDbContext);
        var splitRepository = new SplitRepository(appDbContext);

        var importRecordService = new ImportRecordService(importRecordRepository);
        var transactionService = new TransactionService(transactionRepository, accountServiceMock.Object,
            importRecordService,
            Mock.Of<ILogger<TransactionService>>(), splitRepository);
        _transactionController =
            new TransactionController(auditServiceMock.Object, transactionService, userServiceMock.Object);
        
        var claims = new List<Claim>() 
        { 
            new Claim(ClaimTypes.Email, "mock@example.com"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var context = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
        _transactionController.ControllerContext = context;

        await _transactionController.Import(ImportTransactions.BankImportRequest());
    }

    [Test]
    public async Task TestImportBankTransactions_ShouldImportAll()
    {
        var transactions = (await _transactionController.GetAllAsync()).Payload.ToList();
        Assert.That(transactions, Has.Count.EqualTo(4));

        var amount = transactions.Sum(transaction => transaction.Amount);
        Assert.That(amount, Is.EqualTo(-479667d).Within(0.0001));
    }

    [Test]
    public async Task TestImportDuplicateBankTransactions_ShouldRemainSame()
    {
        await _transactionController.Import(ImportTransactions.BankImportRequest());
        await _transactionController.Import(ImportTransactions.BankImportRequest());

        var transactions = (await _transactionController.GetAllAsync()).Payload.ToList();
        Assert.That(transactions, Has.Count.EqualTo(4));

        var amount = transactions.Sum(transaction => transaction.Amount);
        Assert.That(amount, Is.EqualTo(-479667d).Within(0.0001));
    }

    [Test]
    public async Task TestGetAll_WillGetAllWhenNoPagingSpecified()
    {
        var transactions = (await _transactionController.GetAllAsync()).Payload.ToList();

        Assert.That(transactions, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(transactions.Count(t => t.BankId == "202201041"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202201042"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202201061"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202202211"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task TestGetAll_WillLimitPageSize()
    {
        var pagedResponse = await _transactionController.GetAllAsync(0, 2);
        var transactions = pagedResponse.Payload.ToList();

        Assert.That(pagedResponse.Page, Is.EqualTo(0));
        Assert.That(pagedResponse.PageSize, Is.EqualTo(2));
        Assert.That(pagedResponse.TotalNoOfPages, Is.EqualTo(2));
        Assert.That(pagedResponse.TotalNoOfItems, Is.EqualTo(4));
        Assert.That(transactions, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(transactions.Count(t => t.BankId == "202202211"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202201061"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task TestGetAll_WillOffsetPages()
    {
        var pagedResponse = await _transactionController.GetAllAsync(1, 2);
        var transactions = pagedResponse.Payload.ToList();

        Assert.That(pagedResponse.Page, Is.EqualTo(1));
        Assert.That(pagedResponse.PageSize, Is.EqualTo(2));
        Assert.That(pagedResponse.TotalNoOfPages, Is.EqualTo(2));
        Assert.That(pagedResponse.TotalNoOfItems, Is.EqualTo(4));
        Assert.That(transactions, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(transactions.Count(t => t.BankId == "202201041"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202201042"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task TestGetByDateAsync_WillGetTransactionsWithinDates()
    {
        var startDate = new DateTime(2022, 01, 01);
        var endDate = new DateTime(2022, 01, 05);
        var transactions = (await _transactionController.GetByDateAsync(startDate, endDate)).Payload.ToList();

        Assert.That(transactions, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(transactions.Count(t => t.BankId == "202201041"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202201042"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task TestGetByDateAsync_WillReturnEmptyIfNoMatches()
    {
        var startDate = new DateTime(2021, 01, 01);
        var endDate = new DateTime(2021, 01, 05);
        var transactions = (await _transactionController.GetByDateAsync(startDate, endDate)).Payload.ToList();
        Assert.That(transactions, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task TestGetByDateAsync_WillUseOuterBounds()
    {
        var startDate = new DateTime(2022, 01, 04);
        var endDate = new DateTime(2022, 01, 06);

        var transactions = (await _transactionController.GetByDateAsync(startDate, endDate)).Payload.ToList();
        Assert.That(transactions, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(transactions.Count(t => t.BankId == "202201041"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202201042"), Is.EqualTo(1));
            Assert.That(transactions.Count(t => t.BankId == "202201061"), Is.EqualTo(1));
        });
    }

    /**
     * Split methods
     */
    [Test]
    public async Task TestUpdateSplit_NormalSplitWillBeAccepted()
    {
        var transaction = (await _transactionController.GetAllAsync()).Payload.First(t => t.Amount == 1000);

        await _transactionController.Split(new SplitRequest
        {
            TransactionId = transaction.Id,
            Splits = new SplitLine[]
            {
                new()
                {
                    UserId = 1,
                    Amount = 500,
                    reviewed = false
                },
                new()
                {
                    UserId = 2,
                    Amount = 500,
                    reviewed = false
                }
            }
        });

        var split = (await _transactionController.GetSplitById(transaction.Id)).ToList();

        Assert.That(split, Has.Count.EqualTo(2));
        Assert.That(split.Sum(s => s.Amount), Is.EqualTo(transaction.Amount));

        Assert.Multiple(() =>
        {
            Assert.That(split.Count(s => s.UserId == 1), Is.EqualTo(1));
            Assert.That(split.Count(s => s.UserId == 2), Is.EqualTo(1));
        });

        await _transactionController.DeleteSplit(transaction.Id);
    }

    [Test]
    public async Task TestUpdateSplit_OverSplitWillNotBeAccepted()
    {
        var transaction = (await _transactionController.GetAllAsync()).Payload.First(t => t.Amount == 1000);

        var splitRequest = new SplitRequest
        {
            TransactionId = transaction.Id,
            Splits = new SplitLine[]
            {
                new()
                {
                    UserId = 1,
                    Amount = 500,
                    reviewed = false
                },
                new()
                {
                    UserId = 2,
                    Amount = 1500,
                    reviewed = false
                }
            }
        };

        await _transactionController.Split(splitRequest);

        var split = (await _transactionController.GetSplitById(transaction.Id)).ToList();

        Assert.That(split, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task TestUpdateSplit_NewSplitResultOverwritesExisting()
    {
        var transaction = (await _transactionController.GetAllAsync()).Payload.First(t => t.Amount == 1000);

        await _transactionController.Split(new SplitRequest
        {
            TransactionId = transaction.Id,
            Splits = new SplitLine[]
            {
                new()
                {
                    UserId = 1,
                    Amount = 300,
                    reviewed = false
                },
                new()
                {
                    UserId = 2,
                    Amount = 700,
                    reviewed = false
                }
            }
        });

        var split = (await _transactionController.GetSplitById(transaction.Id)).ToList();

        Assert.That(split, Has.Count.EqualTo(2));
        Assert.That(split.Sum(s => s.Amount), Is.EqualTo(transaction.Amount));

        Assert.Multiple(() =>
        {
            Assert.That(split.Count(s => s.UserId == 1), Is.EqualTo(1));
            Assert.That(split.Count(s => s.UserId == 2), Is.EqualTo(1));
            Assert.That(split.Count(s => s.Amount == 300), Is.EqualTo(1));
            Assert.That(split.Count(s => s.Amount == 700), Is.EqualTo(1));
        });

        await _transactionController.Split(new SplitRequest
        {
            TransactionId = transaction.Id,
            Splits = new SplitLine[]
            {
                new()
                {
                    UserId = 3,
                    Amount = 100,
                    reviewed = false
                },
                new()
                {
                    UserId = 4,
                    Amount = 900,
                    reviewed = false
                }
            }
        });

        var updatedSplit = (await _transactionController.GetSplitById(transaction.Id)).ToList();

        Assert.That(updatedSplit, Has.Count.EqualTo(2));
        Assert.That(updatedSplit.Sum(s => s.Amount), Is.EqualTo(transaction.Amount));

        Assert.Multiple(() =>
        {
            Assert.That(updatedSplit.Count(s => s.UserId == 3), Is.EqualTo(1));
            Assert.That(updatedSplit.Count(s => s.UserId == 4), Is.EqualTo(1));
            Assert.That(updatedSplit.Count(s => s.Amount == 100), Is.EqualTo(1));
            Assert.That(updatedSplit.Count(s => s.Amount == 900), Is.EqualTo(1));
        });

        await _transactionController.DeleteSplit(transaction.Id);
    }

    /**
     * Split methods
     */
    [Test]
    public async Task TestDeleteSplit_DeletesAllSplits()
    {
        var transaction = (await _transactionController.GetAllAsync()).Payload.First(t => t.Amount == 1000);

        await _transactionController.Split(new SplitRequest
        {
            TransactionId = transaction.Id,
            Splits = new SplitLine[]
            {
                new()
                {
                    UserId = 1,
                    Amount = 500,
                    reviewed = false
                },
                new()
                {
                    UserId = 2,
                    Amount = 500,
                    reviewed = false
                }
            }
        });

        var split = (await _transactionController.GetSplitById(transaction.Id)).ToList();

        Assert.That(split, Has.Count.EqualTo(2));
        Assert.That(split.Sum(s => s.Amount), Is.EqualTo(transaction.Amount));

        await _transactionController.DeleteSplit(transaction.Id);

        var updatedSplit = (await _transactionController.GetSplitById(transaction.Id)).ToList();

        Assert.That(updatedSplit, Has.Count.EqualTo(0));
    }
}