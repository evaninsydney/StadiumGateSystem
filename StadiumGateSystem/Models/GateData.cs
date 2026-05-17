using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StadiumGateSystem.Models
{
    public class GateData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Gate { get; set; } = "";
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        [Required]
        public int NumberOfPeople { get; set; } = 0;
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "";
    }
}
