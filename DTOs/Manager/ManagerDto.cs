﻿namespace Hr_System_Demo_3.DTOs.Manager
{
    public class ManagerDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}