using ExpenseReconciliation.Domain.Models;
using ExpenseReconciliation.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseReconciliation.Controllers
{

    [Authorize("API")]
    [Route("/api/[controller]")]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            this._transactionService = transactionService;
        }

        [HttpGet("GetAllAsync")]
        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _transactionService.ListAsync();

        }

        [HttpGet("GetByDateAsync")]
        public async Task<IEnumerable<Transaction>> GetByDateAsync(DateTime startDate, DateTime endDate)
        {
            return await _transactionService.GetByDateAsync(startDate, endDate);

        }

        [HttpGet("GetById")]
        public async Task<Transaction> GetById(int id)
        {
            return await _transactionService.GetByIdAsync(id);

        }

        [HttpPost("Import")]
        public async Task GetAllAsync([FromBody] BankTransactionRequest bankTransactionRequest)
        {
            await _transactionService.ImportAsync(bankTransactionRequest);
        }

        [HttpPost("UpdateSplit")]
        public async Task Split([FromBody] SplitRequest splitRequest)
        {
            await _transactionService.AddSplitAsync(splitRequest);
        }

        [HttpGet("GetSplitById")]
        public async Task<IEnumerable<Split>> GetSplitById(int transactionId)
        {
            return await _transactionService.GetSplitByIdAsync(transactionId);

        }

        [HttpPost("DeleteSplit")]
        public async Task Split([FromBody] int transactionId)
        {
            await _transactionService.DeleteSplitAsync(transactionId);
        }
    }
}