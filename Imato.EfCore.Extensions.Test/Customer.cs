using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imato.EfCore.Extensions.Test
{
    [Table("Customers")]
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double Amount { get; set; }
        public DateTime Created { get; set; }

        [Column("active")]
        public bool IsActive { get; set; }
    }
}