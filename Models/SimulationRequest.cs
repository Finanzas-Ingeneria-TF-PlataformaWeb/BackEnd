namespace MiVivienda.Api.Models;

public class SimulationRequest
{
    public int CustomerId { get; set; }
    public int PropertyId { get; set; }
    public decimal LoanAmount { get; set; }
    public string Currency { get; set; } = "PEN";
    public string RateType { get; set; } = "TEA"; // TEA o TNA
    public decimal AnnualRate { get; set; }
    public string? NominalCapitalization { get; set; }
    public int Years { get; set; }
    public int TotalGraceMonths { get; set; }
    public int PartialGraceMonths { get; set; }
    public DateOnly FirstInstallmentDate { get; set; }
}