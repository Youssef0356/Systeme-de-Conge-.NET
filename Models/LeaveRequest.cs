using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testingdatabase.Models
{

    public class LeaveRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Sold { get; set; } = 22;

        [Required] 
        public string UserId { get; set; }

        [Required] 
        public string Description { get; set; }

        [Required] 
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required] 
        public string Status { get; set; }
            
    }
}