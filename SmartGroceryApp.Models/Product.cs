﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SmartGroceryApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        [Display(Name ="Product Code")]
        public string ISBN { get; set; }

        [Required]
        [Display(Name = "Vendor Name")]
        public string Author { get; set; }

        [Required]
        [Display(Name = "List Price")]
        [Range(1, 3000)]
        public double ListPrice { get; set; }

        [Required]
        [Display(Name = "Price For 1-50")]
        [Range(1, 3000)]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Price For 50+")]
        [Range(1, 3000)]
        public double Price50 { get; set; }


        [Required]
        [Display(Name = "Price For 100+")]
        [Range(1, 3000)]
        public double Price100 { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        // It is the navigation property to the category table
        public Category Category { get; set; }

        //[ValidateNever]
        //public string ImageUrl { get; set; }

        //public int TestProperty { get; set; }


        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; }

    }
}
