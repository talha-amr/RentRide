using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentRide.Models;

public class Car
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Model { get; set; } = string.Empty;

    [Range(1950, 2100)]
    public int Year { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    [Column(TypeName = "decimal(10, 2)")]
    [Display(Name = "Daily rate")]
    public decimal DailyRate { get; set; }

    [Display(Name = "Available for rental")]
    public bool IsAvailable { get; set; } = true;
}
