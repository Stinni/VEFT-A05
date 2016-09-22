using Microsoft.EntityFrameworkCore;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.DataAccess
{
	public class AppDataContext : DbContext, IDbContext
	{
		public AppDataContext(DbContextOptions<AppDataContext> options)
		: base(options)
		{
		}

		public DbSet<Person>              Persons              { get; set; }
		public DbSet<Semester>            Semesters            { get; set; }
		public DbSet<CourseTemplate>      CourseTemplates      { get; set; }
		public DbSet<CourseInstance>      CourseInstances      { get; set; }
		public DbSet<TeacherRegistration> TeacherRegistrations { get; set; }
	}
}