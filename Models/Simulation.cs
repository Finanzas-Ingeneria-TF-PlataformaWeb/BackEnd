namespace MiVivienda.Api.Models;

public class Simulation
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public decimal LoanAmount { get; set; }
    public string Currency { get; set; } = "PEN";

    // TEA / TNA o 'effective' / 'nominal' según lo que mandas del front
    public string RateType { get; set; } = "TEA";
    public decimal AnnualRate { get; set; }
    public string? NominalCapitalization { get; set; }

    public int Years { get; set; }
    public int TotalGraceMonths { get; set; }
    public int PartialGraceMonths { get; set; }

    public decimal MonthlyPayment { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalAmountToPay { get; set; }

    // Indicadores avanzados que SÍ se guardan en la tabla Simulations
    public decimal? Npv { get; set; }        // VAN
    public decimal? IrrAnnual { get; set; }  // TIR anual efectiva
    public decimal? Tcea { get; set; }       // TCEA (puede ser igual a IrrAnnual)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}