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

		[HttpGet]
		public IActionResult GetCoursesBySemester(int? page, string semester = null)
		{
		    if (!page.HasValue)
		    {
		        return BadRequest();
		    }

		    var lang = Request.Headers["Accept-Language"].ToString();
		    var langs = lang.Split(',');

            try
		    {
		        var pageResult = _service.GetCourseInstancesBySemester(page.Value, langs[0], semester);
		        return Ok(pageResult);
		    }
		    catch (AppObjectNotFoundException)
		    {
		        return NotFound();
		    }
		}

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("{id}/teachers")]
		public IActionResult AddTeacher(int id, AddTeacherViewModel model)
		{
            if (model == null || !ModelState.IsValid) return BadRequest(ModelState);

            var result = _service.AddTeacherToCourse(id, model);
			return Created("TODO", result);
		}
	}
}
