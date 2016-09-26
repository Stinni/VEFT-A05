using Microsoft.AspNetCore.Mvc;

using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
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
		    var pageNumber = 0;
		    if (page.HasValue)
		    {
                pageNumber = page.Value;
		    }
		    var lang = Request.Headers["Accept-Language"].ToString();
		    var langs = lang.Split(',');
            // TODO: figure out the requested language (if any!)
            // and pass it to the service provider!
            return Ok(_service.GetCourseInstancesBySemester(langs[0], pageNumber, semester));
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
