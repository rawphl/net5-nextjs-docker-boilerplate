using api.Entities;
using Microsoft.AspNetCore.Mvc;
using api;
using System.Collections.Generic;

namespace api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CommentsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public CommentsController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Get()
		{
			var comment = _context.Comments;

			return Ok(new List<Comment>(){ new Comment(), new Comment(), new Comment()});
		}

		[HttpGet("{id}", Name = "GetCommentById")]
		public IActionResult GetById(int id)
		{
			var comment = _context.Find<Comment>(id);

			return Ok(comment);
		}

		[HttpPost]
		public IActionResult Comment([FromBody] Comment comment)
		{
			_context.Add(comment);

			_context.SaveChanges();

			return CreatedAtRoute(nameof(GetById), new { id = comment.CommentId }, comment);
		}
	}
}