namespace MiVivienda.Api.Models;

public class SimulationResult
{
    public int SimulationId { get; set; }

    public decimal MonthlyPayment { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalAmountToPay { get; set; }
    public List<LoanScheduleItem> Schedule { get; set; } = new();

    // Indicadores avanzados
    public decimal? Npv { get; set; }         // VAN
    public decimal? IrrMonthly { get; set; }  // TIR mensual
    public decimal? IrrAnnual { get; set; }   // TIR efectiva anual
    public decimal? Tcea { get; set; }        // TCEA (puedes usar la misma fórmula que IrrAnnual)
}