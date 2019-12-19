using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class TrainingProgram
    {
        public int Id { get; set; }
        public string TrainingProgramName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MaxAttendees { get; set; }
    }
}
