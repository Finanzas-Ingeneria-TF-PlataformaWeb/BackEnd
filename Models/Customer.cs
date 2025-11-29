using System;

namespace MiVivienda.Api.Models
{
    public class Customer
    {
        public int Id { get; set; }

        // Datos personales
        public string DocumentType { get; set; } = string.Empty;      // dni, ce, passport
        public string DocumentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string? MaritalStatus { get; set; }                     // single, married, etc.
        public int Dependents { get; set; }

        // Contacto
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Address { get; set; }

        // Trabajo
        public string? EmploymentType { get; set; }                   // dependent, independent...
        public string? EmployerName { get; set; }
        public string? JobPosition { get; set; }

        // Economía
        public decimal MonthlyIncome { get; set; }
        public decimal? OtherIncome { get; set; }
        public decimal? FixedExpenses { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}