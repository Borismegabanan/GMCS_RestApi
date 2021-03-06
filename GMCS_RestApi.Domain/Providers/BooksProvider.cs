﻿using GMCS_RestApi.Domain.Common;
using GMCS_RestApi.Domain.Contexts;
using GMCS_RestApi.Domain.Interfaces;
using GMCS_RestApi.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMCS_RestApi.Domain.Providers
{
    public class BooksProvider : IBooksProvider
    {
        private readonly ApplicationContext _applicationContext;

        public BooksProvider(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Получение списка всех книг с полным именем автора.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ReadModelBook>> GetAllBooksAsync()
        {
            return await _applicationContext.Books
                .Join(
                    _applicationContext.Authors,
                    x => x.AuthorId,
                    z => z.Id,
                    (book, author) => new ReadModelBook
                    {
                        Id = book.Id,
                        Name = book.Name,
                        Author = author.FullName,
                        BookStateId = book.BookStateId,
                        InitDate = book.InitDate,
                        PublishDate = book.PublishDate,
                        WhoChanged = book.WhoChanged
                    }).ToListAsync();
        }

        /// <summary>
        /// Получение списка всех книг с полным именем автора.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ReadModelBook>> GetBooksByNameAsync(string name)
        {
            return await _applicationContext.Books.Where(x => x.Name.ToLower().Contains(name.ToLower())).Join(
                _applicationContext.Authors,
                x => x.AuthorId,
                z => z.Id,
                (book, author) => new ReadModelBook
                {
                    Id = book.Id,
                    Name = book.Name,
                    Author = author.FullName,
                    BookStateId = book.BookStateId,
                    InitDate = book.InitDate,
                    PublishDate = book.PublishDate,
                    WhoChanged = book.WhoChanged
                }).ToListAsync();
        }

        public async Task<IEnumerable<ReadModelBook>> GetBooksByMetadataAsync(string metadata)
        {
            metadata = metadata.ToLower();

            return await _applicationContext.Books
                .Join(_applicationContext.Authors, x => x.AuthorId, z => z.Id,
                    (book, author) => new { book, author }).Where(x =>
                      x.book.Name.ToLower() == metadata || x.author.Name.ToLower() == metadata ||
                      x.author.Surname.ToLower() == metadata || x.author.MiddleName.ToLower() == metadata).Select(x =>
                      new ReadModelBook
                      {
                          Id = x.book.Id,
                          Name = x.book.Name,
                          Author = x.author.FullName,
                          BookStateId = x.book.BookStateId,
                          InitDate = x.book.InitDate,
                          PublishDate = x.book.PublishDate,
                          WhoChanged = x.book.WhoChanged
                      }).ToListAsync();
        }

        public async Task<bool> IsBookExistAsync(int bookId)
        {
            return await _applicationContext.Books.AnyAsync(x => x.Id == bookId);
        }

        public async Task<bool> IsBookAuthorExistAsync(int bookId)
        {
            var authorId = (await _applicationContext.Books.FirstOrDefaultAsync(x => x.Id == bookId)).AuthorId;
            return await _applicationContext.Authors.AnyAsync(x => x.Id == authorId);
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _applicationContext.Books.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ReadModelBook> GetBookReadModelByIdAsync(int bookId)
        {
            return await _applicationContext.Books.Where(x => x.Id == bookId).Join(_applicationContext.Authors, x => x.AuthorId, z => z.Id,
                (book, author) => new { book, author }).Select(x =>
                new ReadModelBook
                {
                    Id = x.book.Id,
                    Name = x.book.Name,
                    Author = x.author.FullName,
                    BookStateId = x.book.BookStateId,
                    InitDate = x.book.InitDate,
                    PublishDate = x.book.PublishDate,
                    WhoChanged = x.book.WhoChanged
                }).FirstOrDefaultAsync();
        }

        public async Task<List<Book>> GetBooksByAuthorIdAsync(int authorId)
        {
            return await _applicationContext.Books.Where(x => x.AuthorId == authorId).ToListAsync();
        }
    }
}