﻿using System.ComponentModel.DataAnnotations;

namespace Hr_System_Demo_3.lookups
{
    public class LeaveType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } 
    }
}
