﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public string? ISBN { get; set; }
        [Required]
        public string? Author { get; set; }
        [Required]
        [Range(1, 10_000)]
        public double ListPrice { get; set; }
        [Required]
        [Range(1, 10_000)]
        public double Price { get; set; }
        [Required]
        [Range(1, 10_000)]
        public double Price50 { get; set; }
        [Required]
        [Range(1, 10_000)]
        public double Price100 { get; set; }
        [Required]
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        [Required]
        public int CoverTypeId { get; set; }
        public CoverType? CoverType { get; set; }
    }
}
