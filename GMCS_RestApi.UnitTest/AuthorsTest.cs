﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GMCS_RestAPI.Contracts.Request;
using GMCS_RestAPI.Contracts.Response;
using GMCS_RestAPI.Controllers;
using GMCS_RestApi.Domain.Contexts;
using GMCS_RestApi.Domain.Interfaces;
using GMCS_RestApi.Domain.Models;
using GMCS_RestApi.Domain.Providers;
using GMCS_RestApi.Domain.Services;
using GMCS_RestAPI.Mapping;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GMCS_RestApi.UnitTests
{
    public class AuthorsTest
    {
        private static readonly IMapper Mapper =
            new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper();

        private static readonly Mock<IAuthorsService> AuthorServiceMock = new Mock<IAuthorsService>();
        private static readonly Mock<IAuthorsProvider> AuthorProviderMock = new Mock<IAuthorsProvider>();

        private static readonly ApplicationContext TestContext = new TestDbContext();

        private static readonly IAuthorsProvider AuthorsProvider= new AuthorsProvider(TestContext);
        private static readonly IAuthorsService AuthorsService = new AuthorsService(TestContext, new BooksProvider(TestContext),Mapper);

        private static readonly IEnumerable<Author> DefaultValues = GetTestValueForTestDb();

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
            Assert.Equal(DefaultValues.Count(), ((IEnumerable<AuthorDisplayModel>)objectResult.Value).Count());
        }


        /// <summary>
        /// Тест на не найденого пользователя
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorsByName_NotFoundResultTestAsync()
        {
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var actionResult = await controller.GetAuthorByNameAsync("namename");

            Assert.Equal(new NotFoundResult().StatusCode, ((NotFoundResult) actionResult.Result).StatusCode);

        }

        /// <summary>
        /// Тест на нахождение
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorByName_FoundTestAsync()
        {
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var actionResult = await controller.GetAuthorByNameAsync(DefaultValues.First().Name);
            var objectValue = ((ObjectResult) actionResult.Result).Value;
            var models = ((IEnumerable<AuthorDisplayModel>) objectValue);

            Assert.Equal(DefaultValues.First().FullName, models.First().FullName);
        }

        /// <summary>
        /// тест на добавление автора в БД
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAuthor_SuccessTestAsync()
        {
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var newModel = new DeleteAuthorRequest()
                {BirthDate = DateTime.Now, MiddleName = "test", Surname = "testSurname", Name = "testName"};

            var actionResult = await controller.CreateAuthorAsync(Mapper.Map<DeleteAuthorRequest>(newModel));
            var objectResult = (ObjectResult)actionResult.Result;
            var model = (AuthorDisplayModel)objectResult.Value;

            Assert.NotNull(model);
        }
        /// <summary>
        /// Тест на удачное удаление
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveAuthor_SuccessTestAsync()
        {
            var controller = new AuthorsController(Mapper, AuthorsProvider, AuthorsService);
            var actionResult = await controller.RemoveAuthorAsync(1);
            var oldModel = (AuthorDisplayModel) ((ObjectResult) actionResult.Result).Value;

            Assert.True(oldModel.FullName == DefaultValues.First().FullName);
            
        }

        private static IEnumerable<Author> GetTestValueForTestDb()
        {
            var listOfAuthors = new List<Author>()
            {
                new Author()
                {
                    BirthDate = DateTime.Now,
                    Name = "Test",
                    Surname = "SurnameTest",
                    MiddleName = "MiddTest",
                    FullName = "SurnameTest Test MiddTest"
                },
                new Author()
                {
                    BirthDate = DateTime.Now,
                    Name = "Test2",
                    Surname = "SurnameTest2",
                    MiddleName = "MiddTest2",
                    FullName = "SurnameTest2 Test2 MiddTest2"
                }
            };

            TestContext.Authors.AddRange(listOfAuthors);
            TestContext.SaveChanges();

            return listOfAuthors;
        }
        
    }
}
