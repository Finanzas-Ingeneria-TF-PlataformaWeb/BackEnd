namespace MiVivienda.Api.Models;

public class LoanScheduleItem
{
    public int InstallmentNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal Interest { get; set; }
    public decimal Amortization { get; set; }
    public decimal Payment { get; set; }
    public decimal FinalBalance { get; set; }
}