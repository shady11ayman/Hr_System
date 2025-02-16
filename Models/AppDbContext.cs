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
        public DbSet<ShiftType> ShiftTypes { get; set; }
        public DbSet<ContractType> ContractTypes { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<EmployeeApplication> EmployeeApplications { get; set; }
        public DbSet<ScanRecord> ScanRecords { get; set; }
        public DbSet<Deduction> Deductions { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<SalaryAfterDeductions> SalaryAfterDeductions { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Employee>().ToTable("Employees").HasKey(u => u.empId);
            modelBuilder.Entity<Department>().ToTable("Departments").HasKey(d => d.deptId);
           modelBuilder.Entity<Deduction>().ToTable("Deductions").HasKey(d => d.Id);

            modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.deptId).OnDelete(DeleteBehavior.NoAction);

       

            modelBuilder.Entity<Employee>()
                .HasOne(m=> m.Manager)
                .WithMany(m=> m.Employees)
                .HasForeignKey(m=>m.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LeaveRequest>()
          .HasOne(lr => lr.Employee)
          .WithMany()
          .HasForeignKey(lr => lr.EmployeeId);

            modelBuilder.Entity<Employee>()
    .HasOne(e => e.ShiftType)
    .WithMany(st => st.Employees)
    .HasForeignKey(e => e.ShiftTypeId);

            modelBuilder.Entity<Deduction>()
        .HasOne(d => d.Employee)
        .WithMany()
        .HasForeignKey(d => d.EmployeeId)
        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Deduction>()
     .HasOne(d => d.Manager)
     .WithMany()
     .HasForeignKey(d => d.ManagerId)
     .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalaryAfterDeductions>()
       .HasOne(s => s.Employee)
       .WithMany()
       .HasForeignKey(s => s.empId)
       .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalaryAfterDeductions>()
                .HasOne(s => s.Deduction)
                .WithMany()
                .HasForeignKey(s => s.DeductionId);

            modelBuilder.Entity<Department>()
        .HasOne(d => d.Manager)
        .WithOne(m => m.Department)
        .HasForeignKey<Department>(d => d.ManagerId) // ✅ FK in Department table
        .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Manager>()
        .HasOne(m => m.ShiftType)
        .WithMany()
        .HasForeignKey(m => m.ShiftTypeId)
        .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents multiple cascade paths

            // Manager ↔ ContractType (One-to-Many)
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.ContractType)
                .WithMany()
                .HasForeignKey(m => m.ContractTypeId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents cascade path issues

            // Manager ↔ Position (One-to-Many)
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.Position)
                .WithMany()
                .HasForeignKey(m => m.PositionId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents issues

            // Manager ↔ Direct Manager (Self-Referencing)
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.DirectManager)
                .WithMany()
                .HasForeignKey(m => m.DirectManagerId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Employee>().Property(e => e.ShiftTypeId).HasDefaultValue(0);


         

        }
    }
}
