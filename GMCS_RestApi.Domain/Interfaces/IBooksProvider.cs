﻿using GMCS_RestApi.Domain.Common;
using GMCS_RestApi.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMCS_RestApi.Domain.Interfaces
{
    public interface IBooksProvider
    {
        /// <summary>
        /// Получение списка всех книг с полным именем автора.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ReadModelBook>> GetAllBooksAsync();

        /// <summary>
        /// Получение книг по названию.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<IEnumerable<ReadModelBook>> GetBooksByNameAsync(string name);
        /// <summary>
        /// Получение книг по названию или Имени, или Фамилии или Отчеству автора
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        Task<IEnumerable<ReadModelBook>> GetBooksByMetadataAsync(string metadata);

        /// <summary>
        /// Получение книги по индефикатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Book> GetBookByIdAsync(int id);

        /// <summary>
        /// Показывает есть ли запись в Бд об авторе книги
        /// </summary>
        /// <param name="authorId"></param>
        /// <returns></returns>
        Task<bool> IsBookAuthorExistAsync(int authorId);

        /// <summary>
        /// Получение книг об индефикатору автора
        /// </summary>
        /// <param name="authorId"></param>
        /// <returns></returns>
        Task<List<Book>> GetBooksByAuthorIdAsync(int authorId);

        Task<ReadModelBook> GetBookReadModelByIdAsync(int bookId);
    }
}
