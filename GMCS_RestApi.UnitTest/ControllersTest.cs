﻿using AutoMapper;
using GMCS_RestApi.Domain.Contexts;
using GMCS_RestApi.Domain.Contexts.Tools;
using GMCS_RestApi.Domain.Enums;
using GMCS_RestApi.Domain.Interfaces;
using GMCS_RestApi.Domain.Providers;
using GMCS_RestApi.Domain.Queries;
using GMCS_RestApi.Domain.Services;
using GMCS_RestAPI.Contracts.Request;
using GMCS_RestAPI.Contracts.Response;
using GMCS_RestAPI.Controllers;
using GMCS_RestAPI.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Book = GMCS_RestApi.Domain.Models.Book;
using BookDisplayModel = GMCS_RestAPI.Contracts.Response.BookDisplayModel;
using CreateBookRequest = GMCS_RestAPI.Contracts.Request.CreateBookRequest;

namespace GMCS_RestApi.UnitTests
{
    public class ControllersTest
    {

        private static readonly IMapper Mapper =
            new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper();

        private static readonly ApplicationContext TestContext = new TestDbContext();

        private static readonly IBooksProvider BooksProvider = new BooksProvider(TestContext);
        private static readonly IBooksService BooksService = new BooksService(TestContext, Mapper);
        private static readonly IBookStore BooksStoreService = new BookStoreClient();

        private static readonly IAuthorsProvider AuthorsProvider = new AuthorsProvider(TestContext);
        private static readonly IAuthorsService AuthorsService = new AuthorsService(TestContext, new BooksProvider(TestContext), Mapper);

        public ControllersTest()
        {
            ContextInitializer.InitDataBase(TestContext);
        }


        /// <summary>
        /// Тестирование получения авторов
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthors_TestAsync()
        {
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var actionResult = await controller.GetAllAuthorsAsync();
            var objectResult = (ObjectResult)actionResult.Result;

            Assert.IsAssignableFrom<IEnumerable<AuthorDisplayModel>>(objectResult.Value);
            Assert.NotNull(actionResult);
            Assert.Equal(await TestContext.Authors.CountAsync(), ((IEnumerable<AuthorDisplayModel>)objectResult.Value).Count());
        }

        /// <summary>
        /// Тест на не найденого пользователя
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorsByName_NotFoundResultTestAsync()
        {
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var actionResult = await controller.GetAuthorByNameAsync(Guid.NewGuid().ToString());

            Assert.Equal(new NotFoundResult().StatusCode, ((NotFoundResult)actionResult.Result).StatusCode);

        }

        /// <summary>
        /// Тест на нахождение
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorByName_FoundTestAsync()
        {
            var firstAuthor = await TestContext.Authors.FirstOrDefaultAsync();

            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var actionResult = await controller.GetAuthorByNameAsync(firstAuthor.Name);
            var objectValue = (ObjectResult)actionResult.Result;
            var models = (IEnumerable<AuthorDisplayModel>)objectValue.Value;

            Assert.Equal(firstAuthor.Id, models.First().Id);
        }

        /// <summary>
        /// тест на добавление автора в БД
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAuthor_SuccessTestAsync()
        {
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var newModel = new CreateAuthorRequest()
            { BirthDate = DateTime.Now, MiddleName = Guid.NewGuid().ToString(), Surname = "testSurname", Name = "testName" };

            var actionResult = await controller.CreateAuthorAsync(newModel);
            var objectResult = (ObjectResult)actionResult.Result;
            var model = (AuthorDisplayModel)objectResult.Value;

            Assert.NotNull(model);
            Assert.True(TestContext.Authors.Any(x => x.MiddleName == newModel.MiddleName));
        }

        //[Fact]
        //public async Task AddAuthor_ValidationErrorAsync()
        //{

        //}

        /// <summary>
        /// Тест на удачное удаление
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveAuthor_SuccessTestAsync()
        {
            var idToDelete = await TestContext.Authors.CountAsync();
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            await controller.RemoveAuthorAsync(idToDelete);

            Assert.False(await TestContext.Authors.AnyAsync(x => x.Id == idToDelete));

        }

        /// <summary>
        /// Тест на получение всех книг
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAllBooksTest()
        {
            var controller = new BooksController(BooksProvider, BooksService, Mapper, BooksStoreService);
            var actionResult = await controller.GetAllBooksAsync();
            var objectResult = (ObjectResult)actionResult.Result;
            var models = (IEnumerable<BookDisplayModel>)objectResult.Value;

            Assert.Equal(models.Count(), TestContext.Books.Count());
        }

        /// <summary>
        /// Тест получения книги по имени
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetBooksByNameTest()
        {
            var firstName = (await TestContext.Books.FirstOrDefaultAsync()).Name;
            var partOfName = firstName.Remove(1);

            var controller = new BooksController(BooksProvider, BooksService, Mapper, BooksStoreService);
            var actionResult = await controller.GetBooksByNameAsync(partOfName);
            var objectResult = (ObjectResult)actionResult.Result;
            var models = (IEnumerable<BookDisplayModel>)objectResult.Value;
            var model = models.FirstOrDefault();

            Assert.True(model != null && model.Name == firstName);
        }

        /// <summary>
        /// Тест получения книги по методанным.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetBooksByMetadataTest()
        {
            var author = (await TestContext.Authors.FirstOrDefaultAsync());
            var authorMetadata = author.Name;

            var controller = new BooksController(BooksProvider, BooksService, Mapper, BooksStoreService);
            var actionResult = await controller.GetBooksFromMetadataAsync(authorMetadata);
            var objectResult = (ObjectResult)actionResult.Result;
            var models = (IEnumerable<BookDisplayModel>)objectResult.Value;

            var books = TestContext.Books.Where(x => x.AuthorId == author.Id);
            //authors can have same names
            Assert.True(models.Count() >= books.Count());
            //separated name from fullname, to check
            Assert.True(models.All(x => x.Author.Split(' ')[1] == authorMetadata));
        }

        /// <summary>
        /// Тест смены статуса книги
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ChangeBookStateToInStockTest()
        {
            var bookToTest =
                await TestContext.Books.FirstOrDefaultAsync(x => x.BookStateId != (int)BookStates.InStock);

            if (bookToTest == null)
            {
                bookToTest = new Book()
                {
                    AuthorId = 1,
                    BookStateId = (int)BookStates.Sold,
                    InitDate = DateTime.Now,
                    PublishDate = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                    WhoChanged = Environment.UserName
                };
                await TestContext.AddAsync(bookToTest);
                await TestContext.SaveChangesAsync();
            }

            var controller = new BooksController(BooksProvider, BooksService, Mapper, BooksStoreService);
            await controller.ChangeBookStateToInStockAsync(bookToTest.Id);

            Assert.True((await TestContext.Books.FirstOrDefaultAsync(x => x.Id == bookToTest.Id)).BookStateId ==
                        (int)BookStates.InStock);

        }

        /// <summary>
        /// Тест смены статуса книги
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ChangeBookStateToSoldTest()
        {
            var bookToTest =
                await TestContext.Books.FirstOrDefaultAsync(x => x.BookStateId == (int)BookStates.InStock);

            if (bookToTest == null)
            {
                bookToTest = new Book()
                {
                    AuthorId = 1,
                    BookStateId = (int)BookStates.InStock,
                    InitDate = DateTime.Now,
                    PublishDate = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                    WhoChanged = Environment.UserName
                };
                await TestContext.AddAsync(bookToTest);
                await TestContext.SaveChangesAsync();
            }

            var controller = new BooksController(BooksProvider, BooksService, Mapper, BooksStoreService);
            await controller.ChangeBookStateToSoldAsync(bookToTest.Id);


            Assert.True((await TestContext.Books.FirstOrDefaultAsync(x => x.Id == bookToTest.Id)).BookStateId ==
                        (int)BookStates.Sold);
        }

        /// <summary>
        /// Тест создания книги.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateBookTest()
        {
            var newBookCommand = new CreateBookRequest()
            { AuthorId = 1, Name = Guid.NewGuid().ToString(), PublishDate = DateTime.Now };

            var controller = new BooksController(BooksProvider, BooksService, Mapper, BooksStoreService);
            var actionResult = await controller.CreateBookAsync(newBookCommand);
            var objectResult = (ObjectResult)actionResult.Result;
            var model = (BookDisplayModel)objectResult.Value;

            Assert.True(await TestContext.Books.AnyAsync(x => x.Name == newBookCommand.Name));
            Assert.True(model.Name == newBookCommand.Name);
        }

        /// <summary>
        /// Тест на удаление книги
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveBookByIdTest()
        {
            var lastBook = await TestContext.Books.LastAsync();
            var deleteBookCommand = new BookQuery() { Id = lastBook.Id };

            var controller = new BooksController(BooksProvider, BooksService, Mapper, BooksStoreService);
            var actionResult = await controller.RemoveBookByIdAsync(deleteBookCommand.Id);
            var objectResult = (ObjectResult)actionResult.Result;
            var model = (BookDisplayModel)objectResult.Value;


            Assert.False(await TestContext.Books.AnyAsync(x => x.Id == deleteBookCommand.Id)); //notEmpty
            Assert.True(lastBook.Id == model.Id && lastBook.Name == model.Name);
        }

    }
}
