using System;
using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using CoursesAPI.Services.Services;
using CoursesAPI.Tests.MockObjects;
using Xunit;

namespace CoursesAPI.Tests.Services
{
	public class CourseServicesTests
	{
		private MockUnitOfWork<MockDataContext> _mockUnitOfWork;
		private CoursesServiceProvider _service;
		private List<TeacherRegistration> _teacherRegistrations;

		private const string SSN_DABS    = "1203735289";
		private const string SSN_GUNNA   = "1234567890";
        private const string SSN_KHF     = "0110813209";
        private const string INVALID_SSN = "9876543210";

	    private const string NAME_DABS   = "Daníel B. Sigurgeirsson";
        private const string NAME_GUNNA  = "Guðrún Guðmundsdóttir";
        private const string NAME_KHF    = "Kristinn H. Freysteinsson";

	    private const int COURSEID_VEFT_20143 = 1200;
        private const int COURSEID_VEFT_20153 = 1337;
		private const int COURSEID_VEFT_20163 = 1338;
        private const int COURSEID_PROG_20163 = 1339;
        private const int INVALID_COURSEID    = 9999;

		public CourseServicesTests()
		{
			_mockUnitOfWork = new MockUnitOfWork<MockDataContext>();

			#region Persons
			var persons = new List<Person>
			{
				new Person
				{
					ID    = 1,
					Name  = NAME_DABS,
					SSN   = SSN_DABS,
					Email = "dabs@ru.is"
				},
				new Person
				{
					ID    = 2,
					Name  = NAME_GUNNA,
					SSN   = SSN_GUNNA,
					Email = "gunna@ru.is"
				},
                new Person
                {
                    ID    = 3,
                    Name  = NAME_KHF,
                    SSN   = SSN_KHF,
                    Email = "kristinnf13@ru.is"
                }
			};
			#endregion

			#region Course templates

			var courseTemplates = new List<CourseTemplate>
			{
				new CourseTemplate
				{
					CourseID    = "T-514-VEFT",
					Description = "Í þessum áfanga verður fjallað um vefþj...",
					Name        = "Vefþjónustur"
				},
                new CourseTemplate
                {
                    CourseID    = "T-111-PROG",
                    Description = "Í þessum áfanga verður fjallað um forritun...",
                    Name        = "Forritun"
                }
			};
			#endregion

			#region Courses
			var courses = new List<CourseInstance>
			{
                new CourseInstance
                {
                    ID         = COURSEID_VEFT_20143,
                    CourseID   = "T-514-VEFT",
                    SemesterID = "20143"
                },
				new CourseInstance
				{
					ID         = COURSEID_VEFT_20153,
					CourseID   = "T-514-VEFT",
					SemesterID = "20153"
				},
				new CourseInstance
				{
					ID         = COURSEID_VEFT_20163,
					CourseID   = "T-514-VEFT",
					SemesterID = "20163"
				},
                new CourseInstance
                {
                    ID         = COURSEID_PROG_20163,
                    CourseID   = "T-111-PROG",
                    SemesterID = "20163"
                }
            };
			#endregion

			#region Teacher registrations
			_teacherRegistrations = new List<TeacherRegistration>
			{
				new TeacherRegistration
				{
					ID               = 101,
					CourseInstanceID = COURSEID_VEFT_20153,
					SSN              = SSN_DABS,
					Type             = TeacherType.MainTeacher
				},
                new TeacherRegistration
                {
                    ID               = 102,
                    CourseInstanceID = COURSEID_PROG_20163,
                    SSN              = SSN_KHF,
                    Type             = TeacherType.AssistantTeacher
                }
			};
			#endregion

			_mockUnitOfWork.SetRepositoryData(persons);
			_mockUnitOfWork.SetRepositoryData(courseTemplates);
			_mockUnitOfWork.SetRepositoryData(courses);
			_mockUnitOfWork.SetRepositoryData(_teacherRegistrations);

			_service = new CoursesServiceProvider(_mockUnitOfWork);
		}

		#region GetCoursesBySemester
        /// <summary>
        /// Gets the list of courses for the "20153" semester and checks if it has 1 course
        /// and that the main teacher of that course is DABS.
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsListOfOneCourseAndOneMainTeacher()
        {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester(1, null, "20153"); 

            // Assert:
            Assert.Equal(1, courses.Items.Count);
            Assert.Equal(NAME_DABS, courses.Items[0].MainTeacher);
        }

        /// <summary>
        /// Gets the list of courses for the "20143" semester and checks if it has 1 course
        /// and that the main teacher's name is an empty string.
        /// </summary>
        [Fact]
	    public void GetCoursesBySemester_ReturnsListOfOneCourseAndNoMainTeacher()
	    {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester(1, null, "20143");

            // Assert:
            Assert.Equal(1, courses.Items.Count);
            Assert.Equal("", courses.Items[0].MainTeacher);
        }

        /// <summary>
        /// Gets the list of courses for the "20163" semester and checks if it has 2 courses
        /// and that both main teacher's names are empty strings. And therefore makes sure that
        /// the assistant teacher's name isn't returned as a course's main teacher's name.
        /// </summary>
        [Fact]
        public void GetCoursesBySemester_ReturnsListOfTwoCoursesAndNoMainTeachers()
        {
            // Arrange:

            // Act:
            var courses = _service.GetCourseInstancesBySemester(1, null, "20163");

            // Assert:
            Assert.Equal(2, courses.Items.Count);
            foreach (var c in courses.Items)
            {
                if (c.CourseInstanceID == COURSEID_VEFT_20163)
                {
                    Assert.Equal("", c.MainTeacher);
                }
                if (c.CourseInstanceID == COURSEID_PROG_20163)
                {
                    // Test to make sure that assistant teacher isn't returned as main teacher
                    Assert.Equal("", c.MainTeacher);
                }
            }
        }
        #endregion

        #region AddTeacher

        /// <summary>
        /// Adds a main teacher to a course which doesn't have a
        /// main teacher defined already (see test data defined above).
        /// </summary>
        [Fact]
		public void AddTeacher_WithValidTeacherAndCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			var prevCount = _teacherRegistrations.Count;
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);

			// Assert:

			// Check that the dto object is correctly populated:
			Assert.Equal(SSN_GUNNA, dto.SSN);
			Assert.Equal(NAME_GUNNA, dto.Name);

			// Ensure that a new entity object has been created:
			var currentCount = _teacherRegistrations.Count;
			Assert.Equal(prevCount + 1, currentCount);

			// Get access to the entity object and assert that
			// the properties have been set:
			var newEntity = _teacherRegistrations.Last();
			Assert.Equal(COURSEID_VEFT_20163, newEntity.CourseInstanceID);
			Assert.Equal(SSN_GUNNA, newEntity.SSN);
			Assert.Equal(TeacherType.MainTeacher, newEntity.Type);

			// Ensure that the Unit Of Work object has been instructed
			// to save the new entity object:
			Assert.True(_mockUnitOfWork.GetSaveCallCount() > 0);
		}

		[Fact]
		public void AddTeacher_InvalidCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Assert.Throws<AppObjectNotFoundException>( () => _service.AddTeacherToCourse(INVALID_COURSEID, model) );
		}

		/// <summary>
		/// Ensure it is not possible to add a person as a teacher
		/// when that person is not registered in the system.
		/// </summary>
		[Fact]
		public void AddTeacher_InvalidTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = INVALID_SSN,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Assert.Throws<AppObjectNotFoundException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
		}

		/// <summary>
		/// In this test, we test that it is not possible to
		/// add another main teacher to a course, if one is already
		/// defined.
		/// </summary>
		[Fact]
		public void AddTeacher_AlreadyWithMainTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Exception ex = Assert.Throws<AppValidationException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
			Assert.Equal(ex.Message, "COURSE_ALREADY_HAS_A_MAIN_TEACHER");
		}

		/// <summary>
		/// In this test, we ensure that a person cannot be added as a
		/// teacher in a course, if that person is already registered
		/// as a teacher in the given course.
		/// </summary>
		[Fact]
		public void AddTeacher_PersonAlreadyRegisteredAsTeacherInCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_DABS,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Exception ex = Assert.Throws<AppValidationException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
			Assert.Equal(ex.Message, "PERSON_ALREADY_REGISTERED_AS_TEACHER_IN_COURSE");
		}

		#endregion
	}
}
