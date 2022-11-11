using System.Globalization;
using ExpenseReconciliation.Domain.Models;
using ExpenseReconciliation.Domain.Repositories;
using ExpenseReconciliation.Domain.Services;
using ExpenseReconciliation.Repository;

namespace ExpenseReconciliation.Services;

public class TransactionService
{
    private readonly TransactionRepository _transactionRepository;
    private readonly IAccountService _accountService;
    private readonly ImportRecordService _importRecordService;
    private readonly SplitRepository _splitRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger _logger;

    public TransactionService(TransactionRepository transactionRepository,
        IAccountService accountService,
        ImportRecordService importRecordService, ILogger<TransactionService> logger,
        SplitRepository splitRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _accountService = accountService;
        _importRecordService = importRecordService;
        _splitRepository = splitRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Paged<Transaction>> ListAsync(int page, int pageSize)
    {
        return await _transactionRepository.ListAsync(page, pageSize);
    }

    public async Task<Paged<Transaction>> GetByDateAsync(DateTime startDate, DateTime endDate, int page, int pageSize)
    {
        return await _transactionRepository.GetByDateAsync(startDate, endDate, page, pageSize);
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _transactionRepository.GetById(id);
    }

    public async Task ImportAsync(BankTransactionRequest bankTransactionRequest)
    {
        var accountId = await _accountService.FindOrCreateId(bankTransactionRequest.AccountNumber);
        var importId = await _importRecordService.CreateNewImport(bankTransactionRequest, accountId);

        var transactionList = new List<Transaction>();

        foreach (var line in bankTransactionRequest.Transactions)
        {
            var transaction = new Transaction();
            transaction.Amount = decimal.Parse(line.Amount) * 100;
            transaction.Date = DateTime.ParseExact(line.Date, "yyyyMMdd", CultureInfo.InvariantCulture);
            transaction.Details = line.Name + " " + line.Memo;
            transaction.BankId = line.BankId;
            transaction.AccountId = accountId;
            transaction.ImportId = importId;
            transactionList.Add(transaction);
        }

        await _transactionRepository.AddAsync(transactionList);
        _logger.LogInformation("Parsed {transactionList.Count} transactions records from the file",
            transactionList.Count());
    }

    public async Task AddSplitAsync(SplitRequest splitRequest)
    {
        var splitDb = (await _splitRepository.GetByIdAsync(splitRequest.TransactionId)).ToList();
        if (splitDb.Count > 0)
        {
            await _splitRepository.DeleteSplitAsync(splitRequest.TransactionId);
        }

        foreach (var record in splitRequest.Splits)
        {
            var split = new Split();
            split.Amount = record.Amount;
            split.TransactionId = splitRequest.TransactionId;
            split.UserId = record.UserId;
            await _splitRepository.AddSplitAsync(split);
        }
    }

    public async Task<IEnumerable<Split>> GetSplitByIdAsync(int transactionId)
    {
        return await _splitRepository.GetByIdAsync(transactionId);
    }

    public async Task DeleteSplitAsync(int transactionId)
    {
        await _splitRepository.DeleteSplitAsync(transactionId);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.ListAllAsync();
    }

    public async Task UpdateCategoryAsync(CategoryRequest categoryRequest)
    {
        await _transactionRepository.UpdateCategoryAsync(categoryRequest);
    }
}