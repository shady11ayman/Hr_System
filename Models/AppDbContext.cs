using Hr_System_Demo_3.Day_off_requests;
using Hr_System_Demo_3.lookups;
using Hr_System_Demo_3.Models;
using Microsoft.EntityFrameworkCore;

namespace Hr_System_Demo_3
{
    public class AppDbContext :DbContext
    {

        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
       // public DbSet<ShiftType> ShiftTypes { get; set; }
        public DbSet<ContractType> ContractTypes { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<EmployeeApplication> EmployeeApplications { get; set; }
        public DbSet<ScanRecord> ScanRecords { get; set; }
        public DbSet<Deduction> Deductions { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Employee>().ToTable("Employees").HasKey(u => u.empId);
            modelBuilder.Entity<Department>().ToTable("Departments").HasKey(d => d.deptId);

            modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.deptId);

            modelBuilder.Entity<LeaveRequest>()
          .HasOne(lr => lr.Employee)
          .WithMany()
          .HasForeignKey(lr => lr.EmployeeId);

          /*  modelBuilder.Entity<Employee>()
        .HasOne(e => e.ShiftType)
        .WithMany(s => s.Employees)
        .HasForeignKey(e => e.ShiftTypeId)
        .OnDelete(DeleteBehavior.SetNull); // ✅ Prevents FK conflicts*/
            //modelBuilder.Entity<Product>().ToTable("Products").HasKey(p => p.Id);
            //modelBuilder.Entity<Product>().ToTable("Products")
            //      .HasOne(p => p.User)
            //      .WithMany(u => u.Products)
            //      .HasForeignKey(p => p.UserId);


        }
    }
}
