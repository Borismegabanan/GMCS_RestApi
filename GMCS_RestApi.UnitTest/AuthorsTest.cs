﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GMCS_RestAPI.Contracts.Response;
using GMCS_RestAPI.Controllers;
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
        private static readonly IMapper _mapper = new MapperConfiguration(mc => mc.AddProfile(new MappingProfile())).CreateMapper();
        private static readonly Mock<IAuthorsService> AuthorServiceMock = new Mock<IAuthorsService>();
        private static readonly Mock<IAuthorsProvider> AuthorProviderMock = new Mock<IAuthorsProvider>();
        
        /// <summary>
        /// Тестирование получения авторов
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthors_TestAsync()
        {
            AuthorProviderMock.Setup(x => x.GetAllAuthorsAsync(null)).Returns(GetTestAuthors());

            var controller = new AuthorsController(_mapper, AuthorProviderMock.Object, AuthorServiceMock.Object);
            var actionResult = await controller.Get();
            var objectResult = (ObjectResult)actionResult.Result;

            Assert.IsAssignableFrom<IEnumerable<AuthorResponse>>(objectResult.Value);
            Assert.NotNull(actionResult);
            Assert.Equal(GetTestAuthors().Result.Count(), ((IEnumerable<AuthorResponse>)objectResult.Value).Count());
        }


        /// <summary>
        /// Тест на не найденого пользователя
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorsByName_NotFoundResultTestAsync()
        {
            //Todo обязательно ли создавать отдельный метод?
            AuthorProviderMock.Setup(prov => prov.GetAllAuthorsAsync("TestName"))
                .Returns(GetEmptyList);

            var controller = new AuthorsController(_mapper, AuthorProviderMock.Object, AuthorServiceMock.Object);
            var actionResult = await controller.Get("TestName");

            Assert.Equal(new NotFoundResult().StatusCode, ((NotFoundResult)actionResult.Result).StatusCode);

        }

        /// <summary>
        /// Тест на нахождение
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAuthorByName_FoundTestAsync()
        {
            //Todo какой-то очень ужасный метод
            AuthorProviderMock.Setup(prov => prov.GetAllAuthorsAsync("Carroll"))
                .Returns(GetTestAuthors);

            var controller = new AuthorsController(_mapper, AuthorProviderMock.Object, AuthorServiceMock.Object);
            var actionResult = await controller.Get("Carroll");
            var objectValue = ((ObjectResult) actionResult.Result).Value;
            var models = ((IEnumerable<AuthorResponse>) objectValue);
            
            Assert.Equal("Lewis Carroll",models.First().FullName);
        }

        /// <summary>
        /// тест на добавление
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAuthor_SuccessTestAsync()
        {
            var controller = new AuthorsController(_mapper, AuthorProviderMock.Object, AuthorServiceMock.Object);
            var newModel = GetTestAuthors().Result.First();

            var actionResult = await controller.Post(newModel);
            var objectResult = (ObjectResult) actionResult.Result;
            var model = (Author) objectResult.Value;

            Assert.NotNull(model);
            AuthorServiceMock.Verify(r => r.AddAsync(newModel));
        }

        [Fact]
        public async Task RemoveAuthor_SuccessTestAsync()
        {
            AuthorProviderMock.Setup(prov => prov.GetTheAuthorAsync(0)).Returns(GetTestAuthor);

            var controller = new AuthorsController(_mapper, AuthorProviderMock.Object, AuthorServiceMock.Object);
            var actionResult = await controller.Delete(0);
            var oldModel = (AuthorResponse)((ObjectResult)actionResult.Result).Value;

            Assert.True(oldModel.FullName==GetTestAuthor().Result.FullName);

            //AuthorServiceMock.Verify(r => r.RemoveAsync(GetTestAuthor().Result));
        }

#pragma warning disable 1998
        private async Task<IEnumerable<Author>> GetTestAuthors()
#pragma warning restore 1998
        {
            return new List<Author>
            {
                new Author()
                {
                    BirthDate = new DateTime(1832, 1, 27),
                    Name = "Carroll",
                    MiddleName = "mid",
                    Surname = "Lewis",
                    FullName = "Lewis Carroll"
                },
                new Author()
                {
                    BirthDate = new DateTime(1998, 6, 23),
                    Name = "Борис",
                    MiddleName = "Валентинович",
                    Surname = "Красильников",
                    FullName = "Красильников Борис Валентинович"
                }
            };
        }


#pragma warning disable 1998
        private async Task<Author> GetTestAuthor()
#pragma warning restore 1998
        {
            return new Author()
            {
                BirthDate = new DateTime(1832, 1, 27),
                Name = "Carroll",
                MiddleName = "mid",
                Surname = "Lewis",
                FullName = "Lewis Carroll"
            };
        }

#pragma warning disable 1998
        private async Task<IEnumerable<Author>> GetEmptyList()
#pragma warning restore 1998
        {
            return new List<Author>();
        }
    }
}
