﻿using System.Threading.Tasks;
using GMCS_RestApi.Domain.Commands;
using GMCS_RestApi.Domain.Contexts;
using GMCS_RestApi.Domain.Interfaces;
using GMCS_RestApi.Domain.Models;

namespace GMCS_RestApi.Domain.Services
{
    public class AuthorsService : IAuthorsService
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IBooksProvider _booksProvider;

        public AuthorsService(ApplicationContext applicationContext, IBooksProvider booksProvider)
        {
            _applicationContext = applicationContext;
            _booksProvider = booksProvider;
        }

        public async Task CreateAuthorAsync(CreateAuthorCommand authorCommand)
        {
            var author = new Author(authorCommand);

            await _applicationContext.Authors.AddAsync(author);
            await _applicationContext.SaveChangesAsync();
        }

        public async Task RemoveAuthorAsync(Author author)
        {
            _applicationContext.Authors.Remove(author);

            var bookToRemove = await _booksProvider.GetBooksByAuthorIdAsync(author.Id);

            _applicationContext.RemoveRange(bookToRemove);

            await _applicationContext.SaveChangesAsync();
        }
    }
}