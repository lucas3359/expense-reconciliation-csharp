using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ExpenseReconciliation.Domain.Models;

public class BankTransactionRequest
{
    public string AccountNumber { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public IEnumerable<TransactionLine> Transactions { get; set; }
    
}

public class TransactionLine
{
    
    [JsonPropertyName("TRNTYPE")]
    public string TransactionType { get; set; }
    [JsonPropertyName("DTPOSTED")]

    public string Date { get; set; }
    [JsonPropertyName("TRNAMT")]
    public string Amount { get; set; }
    [JsonPropertyName("FITID")]
    public string BankId { get; set; }
    [JsonPropertyName("NAME")]
    public string Name { get; set; }
    [JsonPropertyName("MEMO")]
    public string Memo { get; set; }

// "TRNTYPE": "FEE",
// "DTPOSTED": "20220331",
// "TRNAMT": "-12.50",
// "FITID": "202203310",
// "NAME": "Monthly A C Fee",
// "MEMO": "Bank Fee"
}