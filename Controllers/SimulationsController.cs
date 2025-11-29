using Microsoft.AspNetCore.Mvc;
using MiVivienda.Api.Data;
using MiVivienda.Api.Models;
using MiVivienda.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace MiVivienda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly LoanCalculatorService _calculator;

    public SimulationsController(AppDbContext context, LoanCalculatorService calculator)
    {
        _context = context;
        _calculator = calculator;
    }

    // POST api/simulations  -> calcula, guarda y devuelve SimulationResult + Id
    [HttpPost]
    public async Task<ActionResult<SimulationResult>> Simulate(SimulationRequest request)
    {
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        var property = await _context.Properties.FindAsync(request.PropertyId);

        if (customer == null || property == null)
        {
            return BadRequest("Cliente o unidad inmobiliaria no encontrada.");
        }

        var result = _calculator.CalculateFrenchSchedule(request);

        var simulation = new Simulation
        {
            CustomerId = customer.Id,
            PropertyId = property.Id,
            LoanAmount = request.LoanAmount,
            Currency = request.Currency,
            RateType = request.RateType,
            AnnualRate = request.AnnualRate,
            NominalCapitalization = request.NominalCapitalization,
            Years = request.Years,
            TotalGraceMonths = request.TotalGraceMonths,
            PartialGraceMonths = request.PartialGraceMonths,
            MonthlyPayment = result.MonthlyPayment,
            TotalInterest = result.TotalInterest,
            TotalAmountToPay = result.TotalAmountToPay,
            Npv = result.Npv,
            IrrAnnual = result.IrrAnnual,
            Tcea = result.Tcea
        };

        _context.Simulations.Add(simulation);
        await _context.SaveChangesAsync();

        // Injectar el Id en el resultado que enviamos al front
        result.SimulationId = simulation.Id;

        return Ok(result);
    }

    // GET api/simulations/{id} -> devuelve exactamente lo que espera tu vista de resultado
    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetSimulation(int id)
    {
        var simulation = await _context.Simulations.FindAsync(id);
        if (simulation == null)
        {
            return NotFound();
        }

        var request = new SimulationRequest
        {
            CustomerId = simulation.CustomerId,
            PropertyId = simulation.PropertyId,
            LoanAmount = simulation.LoanAmount,
            Currency = simulation.Currency,
            RateType = simulation.RateType,
            AnnualRate = simulation.AnnualRate,
            NominalCapitalization = simulation.NominalCapitalization,
            Years = simulation.Years,
            TotalGraceMonths = simulation.TotalGraceMonths,
            PartialGraceMonths = simulation.PartialGraceMonths,
            FirstInstallmentDate = DateOnly.FromDateTime(DateTime.Today)
        };

        var result = _calculator.CalculateFrenchSchedule(request);

        var summary = new
        {
            loanAmount = simulation.LoanAmount,
            currency = simulation.Currency,
            annualRate = simulation.AnnualRate / 100m,
            termYears = simulation.Years,
            termMonths = simulation.Years * 12,
            monthlyInstallment = result.MonthlyPayment,
            totalInterest = result.TotalInterest,
            totalPayable = result.TotalAmountToPay,
            npv = result.Npv,
            irrAnnual = result.IrrAnnual,
            tcea = result.Tcea
        };

        var schedule = result.Schedule.Select(item => new
        {
            installmentNumber = item.InstallmentNumber,
            date = item.DueDate.ToDateTime(TimeOnly.MinValue),
            initialBalance = item.InitialBalance,
            interest = item.Interest,
            amortization = item.Amortization,
            installment = item.Payment,
            finalBalance = item.FinalBalance
        });

        return Ok(new { summary, schedule });
    }
    // GET api/simulations?customerId=123 (customerId opcional)
// Devuelve un listado resumido para el historial
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? customerId = null)
    {
        var query = _context.Simulations
            .Include(s => s.Customer)
            .Include(s => s.Property)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == customerId.Value);
        }

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Take(100) // por ejemplo, las últimas 100
            .Select(s => new
            {
                id = s.Id,
                createdAt = s.CreatedAt,
                customerName = s.Customer != null ? s.Customer.FullName : string.Empty,
                propertyName = s.Property != null
                    ? (s.Property.Name ?? s.Property.Code ?? string.Empty)
                    : string.Empty,
                loanAmount = s.LoanAmount,
                currency = s.Currency,
                years = s.Years,
                annualRate = s.AnnualRate / 100m,   // fracción: 0.105
                monthlyInstallment = s.MonthlyPayment,
                totalPayable = s.TotalAmountToPay,
                irrAnnual = s.IrrAnnual,
                tcea = s.Tcea
            })
            .ToListAsync();

        return Ok(items);
    }

    
}
