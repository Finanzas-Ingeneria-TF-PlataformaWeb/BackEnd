using MiVivienda.Api.Models;

namespace MiVivienda.Api.Services;

public class LoanCalculatorService
{
    public SimulationResult CalculateFrenchSchedule(SimulationRequest request)
    {
        if (request.Years <= 0)
            throw new ArgumentException("Years must be greater than zero.", nameof(request.Years));

        if (request.LoanAmount <= 0)
            throw new ArgumentException("LoanAmount must be greater than zero.", nameof(request.LoanAmount));

        var months = request.Years * 12;

        // Tasa mensual según TEA / TNA
        var monthlyRate = GetMonthlyRate(
            request.RateType,
            request.AnnualRate,
            request.NominalCapitalization
        );

        var monthlyPayment = CalculateMonthlyPayment(
            request.LoanAmount,
            monthlyRate,
            months
        );

        var schedule = new List<LoanScheduleItem>();
        var balance = request.LoanAmount;
        var totalInterest = 0m;
        var date = request.FirstInstallmentDate;

        for (var k = 1; k <= months; k++)
        {
            var interest = Math.Round(balance * monthlyRate, 2);
            var amortization = Math.Round(monthlyPayment - interest, 2);

            if (amortization < 0)
                amortization = 0;

            var finalBalance = Math.Round(balance - amortization, 2);
            if (finalBalance < 0)
                finalBalance = 0;

            schedule.Add(new LoanScheduleItem
            {
                InstallmentNumber = k,
                DueDate = date,
                InitialBalance = Math.Round(balance, 2),
                Interest = interest,
                Amortization = amortization,
                Payment = Math.Round(monthlyPayment, 2),
                FinalBalance = finalBalance
            });

            balance = finalBalance;
            totalInterest += interest;
            date = date.AddMonths(1);
        }

        // Flujos de caja para VAN / TIR
        var cashFlows = new List<decimal>();

        // Flujo inicial: entra el préstamo (positivo)
        cashFlows.Add(request.LoanAmount);

        // Salen las cuotas mensuales (negativo)
        for (var k = 0; k < months; k++)
        {
            cashFlows.Add(-Math.Round(monthlyPayment, 2));
        }

        // VAN al rate mensual calculado
        var npv = CalculateNpv(cashFlows, monthlyRate);
        // TIR mensual (búsqueda binaria, rango acotado, sin double->decimal raros)
        var irrMonthly = CalculateIrrMonthly(cashFlows);
        // TIR efectiva anual (TCEA aproximada)
        var irrEffectiveAnnual = CalculateEffectiveAnnualFromMonthly(irrMonthly);

        return new SimulationResult
        {
            SimulationId = 0,
            MonthlyPayment = Math.Round(monthlyPayment, 2),
            TotalInterest = Math.Round(totalInterest, 2),
            TotalAmountToPay = Math.Round(request.LoanAmount + totalInterest, 2),
            Schedule = schedule,
            Npv = Math.Round(npv, 2),
            IrrMonthly = irrMonthly,
            IrrAnnual = irrEffectiveAnnual,
            Tcea = irrEffectiveAnnual // por ahora TCEA = TIR anual; luego puedes ajustar fórmula si quieres
        };
    }

    private decimal GetMonthlyRate(string rateType, decimal annualRatePercent, string? nominalCapitalization)
    {
        // annualRatePercent viene como 10.5, 8.75, etc.
        var annualRate = annualRatePercent / 100m;

        // TEA o equivalente "effective" (lo que manda el front)
        if (string.Equals(rateType, "TEA", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rateType, "effective", StringComparison.OrdinalIgnoreCase))
        {
            var monthlyDouble = Math.Pow(1 + (double)annualRate, 1.0 / 12.0) - 1.0;
            return (decimal)monthlyDouble;
        }

        // TNA -> efectiva anual -> mensual, según capitalización
        var periodsPerYear = nominalCapitalization?.ToLower() switch
        {
            "monthly" => 12,
            "bimonthly" => 6,
            "quarterly" => 4,
            "semiannual" => 2,
            "annual" => 1,
            _ => 12
        };

        var nominalPerPeriod = annualRate / periodsPerYear;
        var effectiveAnnual =
            (decimal)(Math.Pow(1 + (double)nominalPerPeriod, periodsPerYear) - 1.0);
        var monthlyFromNominal =
            (decimal)(Math.Pow(1 + (double)effectiveAnnual, 1.0 / 12.0) - 1.0);

        return monthlyFromNominal;
    }

    private decimal CalculateMonthlyPayment(decimal principal, decimal monthlyRate, int months)
    {
        if (months <= 0)
            throw new ArgumentOutOfRangeException(nameof(months));

        if (monthlyRate == 0)
            return principal / months;

        var r = (double)monthlyRate;
        var p = (double)principal;
        var n = months;

        var cuota = p * r / (1 - Math.Pow(1 + r, -n));
        return (decimal)cuota;
    }

    // VAN completamente en decimal
    private decimal CalculateNpv(IReadOnlyList<decimal> cashFlows, decimal monthlyRate)
    {
        if (cashFlows.Count == 0)
            return 0m;

        var npv = 0m;
        var discountFactor = 1m;
        var onePlusRate = 1m + monthlyRate;

        for (var t = 0; t < cashFlows.Count; t++)
        {
            if (t == 0)
            {
                npv += cashFlows[t];
            }
            else
            {
                discountFactor *= onePlusRate;
                if (discountFactor == 0)
                    continue;

                npv += cashFlows[t] / discountFactor;
            }
        }

        return npv;
    }

    // TIR mensual por búsqueda binaria, rango [-90 %, 10 %] mensual, para evitar overflows
    private decimal CalculateIrrMonthly(IReadOnlyList<decimal> cashFlows)
    {
        if (cashFlows.Count < 2)
            return 0m;

        // Rango razonable para préstamos:
        // 0.0  =>   0 % mensual
        // 0.1  =>  10 % mensual (~214% EA)
        // No probamos tasas negativas para evitar overflow.
        var low = 0.0m;
        var high = 0.10m;
        var mid = 0.0m;

        for (var i = 0; i < 60; i++)
        {
            mid = (low + high) / 2m;
            var npv = CalculateNpv(cashFlows, mid);

            // Si el VAN está muy cerca de 0, tomamos esa TIR
            if (Math.Abs(npv) < 0.0000001m)
                return mid;

            // Si el VAN > 0, necesitamos una tasa más alta
            if (npv > 0)
                low = mid;
            else
                high = mid;
        }

        return mid;
    }


    private decimal CalculateEffectiveAnnualFromMonthly(decimal monthlyRate)
    {
        if (monthlyRate <= -1m)
            return 0m;

        var onePlus = 1.0 + (double)monthlyRate;
        var ea = Math.Pow(onePlus, 12.0) - 1.0;
        return (decimal)ea;
    }
}
