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
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="semester"></param>
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
