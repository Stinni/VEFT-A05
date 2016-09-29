using System;
using Microsoft.AspNetCore.Mvc;

using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Services;

namespace CoursesAPI.Controllers
{
	[Route("api/courses")]
	public class CoursesController : Controller
	{
		private readonly CoursesServiceProvider _service;

		public CoursesController(IUnitOfWork uow)
		{
			_service = new CoursesServiceProvider(uow);
		}

        /// <summary>
        /// Get method for the "api/courses" route
        /// The 2 parameters are optional, if page is left out then page 1 is returned.
        /// If semester is left out, "20163" is returned. This method returns either a
        /// corresponding error message if the page or semester doesn't exist or a PageResult
        /// with up to 10 CourseInsanceDTO's along with info about number of items, pages, page
        /// size and the page number of the page being returned.
        /// </summary>
        /// <param name="page">The page number being requested.</param>
        /// <param name="semester">The semester being requested.</param>
		[HttpGet]
		public IActionResult GetCoursesBySemester(int? page, string semester = null)
		{
		    var thePage = 1;
		    if (page.HasValue)
		    {
                thePage = page.Value;
		    }

		    var lang = Request.Headers["Accept-Language"].ToString();
		    var langs = lang.Split(',');

            try
		    {
		        var pageResult = _service.GetCourseInstancesBySemester(thePage, langs[0], semester);
		        return Ok(pageResult);
		    }
		    catch (AppObjectNotFoundException)
		    {
		        return NotFound();
		    }
		}

        /// <summary>
        /// This was not implemented for this project and is therefore not
        /// 'pretty' or documented
        /// </summary>
        /// <param name="id">The id of the course that the teacher's going to be teaching</param>
        /// <param name="model">An AddTeacherViewModel including the teacher's SSN and type</param>
        [HttpPost]
		[Route("{id}/teachers")]
		public IActionResult AddTeacher(int id, AddTeacherViewModel model)
		{
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

		    try
		    {
		        var result = _service.AddTeacherToCourse(id, model);
		        return Created("TODO", result);
		    }
		    catch (AppObjectNotFoundException)
		    {
		        return NotFound();
		    }
		    catch (AppValidationException)
		    {
		        return BadRequest();
		    }
		}
	}
}
