using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartGroceryApp.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        [ValidateNever]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime ShippingDate { get; set; }

        public double OrderTotal { get; set; }

        public string? OrderStatus { get; set; }

        public string? PaymentStatus { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Carrier { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTime PaymentDueDate { get; set; }


        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }


        [Required]
        public string StreetAddress { get; set; }


        [Required]
        public string City { get; set; }



        [Required]
        public string State { get; set; }


        [Required]
        public string PostalCode { get; set; }


        [Required]
        public string Name { get; set; }


		[Required]
		public string CardHolderName { get; set; }

		[Required]
        [CreditCard]
		public string CardNumber { get; set; }


		[Required]
        [RegularExpression("^(0[1-9]|1[0-2])/?([0-9]{4}|[0-9]{2})$")]
		public string CardExpiry { get; set; }

		[Required]
		public string CardCVV { get; set; }

		[Required]
		public string CardType { get; set; }

		[Required]
		public string PaymentType { get; set; }






	}
}
