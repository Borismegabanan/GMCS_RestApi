﻿using System;
using System.ComponentModel.DataAnnotations;
using GMCS_RestApi.Domain.Commands;

namespace GMCS_RestApi.Domain.Models
{
    public class Author
    {
        [Key] 
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string MiddleName { get; set; }

        public string FullName { get; set; }

        public DateTime BirthDate { get; set; }

        public Author()
        {
        }

        public Author(CreateAuthorCommand command)
        {
            Name = command.Name;
            Surname = command.Surname;
            MiddleName = command.MiddleName;

            BirthDate = command.BirthDate;

            FullName = command.FullName;
        }
    }
}
