using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using CoursesAPI.Services.Utilities;

namespace CoursesAPI.Services.Services
{
	public class CoursesServiceProvider
	{
    	private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
            _uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

        /// <summary>
        /// You should implement this function, such that all tests will pass.
        /// ---
        /// Implemented so that at first we make sure that both course and person exist. If not an AppObjectNotFoundException
        /// is thrown. If both exist, more checks are made to check if we're updating a teacher or adding a new one.
        /// In the case that we're adding a main teacher and there already exists a main teacher, an AppValidationException
        /// is thrown. Also if a teacher is being added that already exists, an AppValidationException is thrown.
        /// </summary>
        /// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
        /// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
        /// <returns>Should return basic information about the person.</returns>
        /// <exception cref="AppObjectNotFoundException" />
        /// <exception cref="AppValidationException" />

        public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
		    var person = GetPersonBySSNIfExistsNullIfNot(model.SSN);
            if (!CheckIfCourseExists(courseInstanceID) || person == null) throw new AppObjectNotFoundException();

		    var teachers = (from tr in _teacherRegistrations.All()
		                    where tr.CourseInstanceID == courseInstanceID
		                    select tr).ToList();

		    string mainTeacherSSN = null;
            TeacherRegistration teacher = null;

		    foreach (var t in teachers)
		    {
		        if (model.Type == TeacherType.MainTeacher && t.Type == TeacherType.MainTeacher) mainTeacherSSN = t.SSN;
		        if (model.SSN == t.SSN) teacher = t;
            }

		    if (model.Type == TeacherType.MainTeacher) // Skrá á main teacher
		    {
		        if (teacher != null) // Þessi aðili er skráður sem kennari í þessum kúrs
		        {
		            if (mainTeacherSSN != null) // Skráður er main teacher
		            {
		                if (model.SSN != mainTeacherSSN) // Main teacher er ekki sá sami
		                {
		                    throw new AppValidationException("COURSE_ALREADY_HAS_A_MAIN_TEACHER");
		                }
		            }
		            else // Ekki er skráður main teacher
		            {
		                teacher.Type = model.Type; // Teacher type updated
		                _uow.Save();
		            }
		            return person;
		        }
		        if (mainTeacherSSN != null)
		        {
		            throw new AppValidationException("COURSE_ALREADY_HAS_A_MAIN_TEACHER");
		        }
		    }
            else // Skrá á (ekki main) kennara
		    {
		        if (teacher != null) // Þessi aðili er skráður sem kennari í þessum kúrs
		        {
		            if (mainTeacherSSN == null || model.SSN != mainTeacherSSN)
		                throw new AppValidationException("PERSON_ALREADY_REGISTERED_AS_TEACHER_IN_COURSE");
		            teacher.Type = model.Type; // Teacher type updated
                    _uow.Save();
		            return person;
		        }
		    }


		    _teacherRegistrations.Add(new TeacherRegistration
            {
                SSN = model.SSN,
                CourseInstanceID = courseInstanceID,
                Type = model.Type
            });
		    _uow.Save();

            return person;
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public PageResult<CourseInstanceDTO> GetCourseInstancesBySemester(string lang, int page, string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20163";
			}

			var courses = (from c in _courseInstances.All()
				join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
                where c.SemesterID == semester
				select new CourseInstanceDTO
				{
					Name               = lang == "is-IS" ? ct.Name : ct.NameEN,
					TemplateID         = ct.CourseID,
					CourseInstanceID   = c.ID
                }).ToList();

		    foreach (var c in courses)
		    {
		        c.MainTeacher = GetMainTeacherNameOrEmptyString(c.CourseInstanceID);
		    }

            return new PageResult<CourseInstanceDTO>
            {
                Items = courses,
                Paging = new PageInfo
                {
                    PageCount = 1,
                    PageNumber = 1,
                    PageSize = 2,
                    TotalNumberOfItems = 2
                }
            };
		}

        /// <summary>
        /// Checks if a course exists with a certain ID
        /// </summary>
        /// <param name="courseInstanceID">The ID of the course being checked on</param>
        /// <returns>True if course exists or false if it doesn't</returns>
	    private bool CheckIfCourseExists(int courseInstanceID)
	    {
	        var course = (from c in _courseInstances.All()
	                      where c.ID == courseInstanceID
	                      select c).SingleOrDefault();
	        return course != null;
	    }

        /// <summary>
        /// Checks the database for a person with 'ssn' as his/her SSN and
        /// return either null if that person doesn't exists or a PersonDTO
        /// with details about that person
        /// </summary>
        /// <param name="ssn">The SSN of the person that we want details about</param>
        /// <returns>Either null or a PersonDTO object with the person's details</returns>
        private PersonDTO GetPersonBySSNIfExistsNullIfNot(string ssn)
	    {
	        var person = (from p in _persons.All()
	                      where p.SSN == ssn
	                      select new PersonDTO
	                      {
                              SSN = p.SSN,
                              Name = p.Name
	                      }).SingleOrDefault();
	        return person;
	    }

        /// <summary>
        /// A helper function for the GetCourseInstancesBySemester function. Gets the main teacher's name
        /// for a certain course instance or returns an empty string if there's no main teacher registered.
        /// </summary>
        /// <param name="CourseInstanceID">The ID of the course instance which main teacher's name we want.</param>
        /// <returns>A teacher's name or an empty string</returns>
	    private string GetMainTeacherNameOrEmptyString(int CourseInstanceID)
	    {
	        var name = (from tr in _teacherRegistrations.All()
	                    join p in _persons.All() on tr.SSN equals p.SSN
	                    where tr.CourseInstanceID == CourseInstanceID && tr.Type == TeacherType.MainTeacher
	                    select p.Name).SingleOrDefault();

	        return name ?? "";
	    }
	}
}
