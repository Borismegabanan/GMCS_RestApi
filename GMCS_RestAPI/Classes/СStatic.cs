﻿using System;
using System.Linq;
using GMCS_RestAPI.Database;
using GMCS_RestAPI.Models;

namespace GMCS_RestAPI.Classes
{
	public static class СStatic
	{
		public static void InitDataBase(ApplicationContext context)
		{
			if (!context.BookStates.Any())
			{

				context.BookStates.Add(new BookState
				{
					Name = "Продана"
				});

				context.BookStates.Add(new BookState
				{
					Name = "В наличии"
				});

				context.BookStates.Add(new BookState
				{
					Name = "Неизвестно"
				});
				context.SaveChanges();
			}

			if (!context.Authors.Any())
			{
				context.Authors.Add(new Author()
				{
					BirthDate = new DateTime(1832, 1, 27),
					Name = "Carroll",
					MiddleName = "mid",
					Surname = "Lewis",
					FullName = "Lewis Carroll"
				});

				context.Authors.Add(new Author()
				{
					BirthDate = new DateTime(1964, 7, 27),
					Name = "Jeffrey",
					MiddleName = "mid",
					Surname = "Richter",
					FullName = "Richter Jeffrey"
				});

				context.SaveChanges();
			}

			if (!context.Books.Any())
			{
				context.Books.Add(new Book()
				{
					AuthorId = 1,
					Name = "C#",
					InitDate = DateTime.Now,
					PublishDate = DateTime.Now,
					BookStateId = 2,
					WhoChanged = "User1"
				});

				context.Books.Add(new Book()
				{
					AuthorId = 2,
					Name = "Alice's Adventures in Wonderland",
					InitDate = DateTime.Now,
					PublishDate = new DateTime(1965, 11, 26),
					BookStateId = 2,
					WhoChanged = "User1"
				});

				context.Books.Add(new Book()
				{
					AuthorId = 2,
					Name = "Alice in Wonderland ",
					InitDate = DateTime.Now,
					PublishDate = new DateTime(1965, 11, 26),
					BookStateId = 1,
					WhoChanged = "User1"
				});

				context.SaveChanges();
			}
		}
	}
}