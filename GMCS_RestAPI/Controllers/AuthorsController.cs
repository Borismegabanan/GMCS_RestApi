﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMCS_RestApi.Domain.Contexts;
using GMCS_RestApi.Domain.Models;
using GMCS_RestApi.Domain.Providers;
using GMCS_RestApi.Domain.Services;

namespace GMCS_RestAPI.Controllers
{

	[ApiController]
	[Route("[controller]")]
	public class AuthorsController : Controller
	{
		private readonly ApplicationContext _context;
		private readonly IAuthorsProvider _authorsProvider;
		private readonly IAuthorsService _authorsService;

		public AuthorsController(ApplicationContext context, IAuthorsProvider authorsProvider, IAuthorsService authorsService)
		{
			_context = context;
			_authorsProvider = authorsProvider;
			_authorsService = authorsService;
		}

		/// <summary>
		/// Получение списка авторов.
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Author>>> Get()
		{
			var authors = await _authorsProvider.GetAllAuthors();

			return new ObjectResult(authors);
		}

		/// <summary>
		/// Получение автора по его имени.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		[HttpGet("{name}")]
		public async Task<ActionResult<IEnumerable<Author>>> Get(string name)
		{
			var authors = await _authorsProvider.GetAllAuthors(name);
			if (!authors.Any())
			{
				return NotFound();
			}

			return new ObjectResult(authors);
		}

		/// <summary>
		/// Создание автора.
		/// </summary>
		/// <param name="author"></param>
		/// <returns></returns>
		[HttpPost]
#pragma warning disable 1998
		public async Task<ActionResult<Author>> Post(Author author)
#pragma warning restore 1998
		{
			if (author == null)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			author.FullName ??= $"{author.Surname} {author.Name} {author.MiddleName}";

			return Ok(author);
		}

		/// <summary>
		/// Удаление автора и всех его книг.
		/// </summary>
		/// <param name="authorId"></param>
		/// <returns></returns>
		[HttpDelete]
#pragma warning disable 1998
		public async Task<ActionResult<Author>> Delete(int authorId)
#pragma warning restore 1998
		{
			var author = _context.Authors.FirstOrDefault(x => x.Id == authorId);
			if (author == null)
			{
				return NotFound("не найден автор");
			}

			_authorsService.Delete(author);

			return Ok(author);
		}
	}
}
