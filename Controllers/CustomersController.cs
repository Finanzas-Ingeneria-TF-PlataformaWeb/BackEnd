using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiVivienda.Api.Data;
using MiVivienda.Api.Models;

namespace MiVivienda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _db.Customers.AsNoTracking().ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(Customer customer)
    {
        customer.Id = 0;
        customer.CreatedAt = DateTime.UtcNow;

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Customer customer)
    {
        if (id != customer.Id) return BadRequest();

        var existing = await _db.Customers.FindAsync(id);
        if (existing is null) return NotFound();

        // Copiar propiedades
        existing.DocumentType = customer.DocumentType;
        existing.DocumentNumber = customer.DocumentNumber;
        existing.FullName = customer.FullName;
        existing.BirthDate = customer.BirthDate;
        existing.MaritalStatus = customer.MaritalStatus;
        existing.Dependents = customer.Dependents;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.City = customer.City;
        existing.Address = customer.Address;
        existing.EmploymentType = customer.EmploymentType;
        existing.EmployerName = customer.EmployerName;
        existing.JobPosition = customer.JobPosition;
        existing.MonthlyIncome = customer.MonthlyIncome;
        existing.OtherIncome = customer.OtherIncome;
        existing.FixedExpenses = customer.FixedExpenses;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Customers.FindAsync(id);
        if (existing is null) return NotFound();

        _db.Customers.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
