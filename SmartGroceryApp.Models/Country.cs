using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartGroceryApp.Models
{
    public class Country
    {

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

       
    }
}
