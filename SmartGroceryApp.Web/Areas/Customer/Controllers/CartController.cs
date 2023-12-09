using SmartGroceryApp.DataAccess.Repository.IRepository;
using SmartGroceryApp.Models;
using SmartGroceryApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartGroceryApp.Models.ViewModels;
using Stripe;
using Stripe.Checkout;
using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace SmartGroceryApp.Web.Areas.Customer.Controllers
{

    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

      

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; } 

        public CartController (IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
         
        }


        public IActionResult Index()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties:"Product"),
                OrderHeader = new()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
             cart.Product.ProductImages = productImages.Where(u=>u.ProductId == cart.Product.Id).ToList();
             cart. Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }


			ShoppingCartVM.CardTypeList = _unitOfWork.CardType.GetAll().Select(i => new SelectListItem
			{
				Text = i.Name,
				Value = i.Id.ToString()
			});



			if (ShoppingCartVM.ShoppingCartList.Count()>0)
            {
                return View(ShoppingCartVM);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

           
        }

        public IActionResult Summary()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            ShoppingCartVM.CardTypeList = _unitOfWork.CardType.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });


			foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

			


			return View(ShoppingCartVM);
        }


        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{

            

            var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


			ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
			includeProperties: "Product");

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            ShoppingCartVM.CardTypeList = _unitOfWork.CardType.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });



            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


            if (ModelState.IsValid)
            {
                var cardtype = ShoppingCartVM.CardTypeList.Where(x => x.Value == ShoppingCartVM.OrderHeader.CardType).FirstOrDefault();
                if (cardtype.Text == "Master Card" || cardtype.Text == "VISA")
                {
                    if (ShoppingCartVM.OrderHeader.CardNumber.Length != 16)
                    {
                        ModelState.AddModelError("ShoppingCartVM.OrderHeader.CardNumber", "Master Card or Visa  card lenght should be 16 digits");
                        return View(ShoppingCartVM);
                    }

                }
                else
                {
                    if (ShoppingCartVM.OrderHeader.CardNumber.Length != 15)
                    {
                        ModelState.AddModelError("CardNumber", "Master Card or Visa  card lenght should be 16 digits");
                        return View(ShoppingCartVM);
                    }
                }

                var cardexpirymonth = Convert.ToInt32(ShoppingCartVM.OrderHeader.CardExpiry.Split('/')[0]);

                var cardexpiryyear = Convert.ToInt32(ShoppingCartVM.OrderHeader.CardExpiry.Split('/')[1]);
                if (!(cardexpiryyear>=2016 && cardexpiryyear<=2034))
                {

                    ModelState.AddModelError("OrderHeader.CardExpiry", "Expiry year between 2016 to 2034");
                    return View(ShoppingCartVM);

                }

                if (!(cardexpirymonth>=0 && cardexpirymonth <13))
                {

                    ModelState.AddModelError("OrderHeader.CardExpiry", "Expiry month between 01 to 12");
                    return View(ShoppingCartVM);

                }



            }


            if (ModelState.IsValid)
            {



                //ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                //ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                //ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
                //ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                //ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                //ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

                if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    //it is a regular customer account and we need to capture payment
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

                }
                else
                {
                    // it is a company user.
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;

                }

                _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {

                    OrderDetail orderDetail = new()
                    {
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        Price = cart.Price,
                        Count = cart.Count

                    };
                    _unitOfWork.OrderDetail.Add(orderDetail);
                    _unitOfWork.Save();

                }

                if (ShoppingCartVM.OrderHeader.PaymentType == "Normal")
                {
                    Random rnd = new Random();
                    var Transactionid = rnd.Next(); ;
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, Transactionid.ToString(), Transactionid.ToString());
                    _unitOfWork.Save();

                    _unitOfWork.OrderHeader.UpdateStatus(ShoppingCartVM.OrderHeader.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                }
                else
                {
                    if (applicationUser.CompanyId.GetValueOrDefault() == 0)
                    {
                        //it is a regular customer account and we need to capture payment
                        //Stripe Logic
                        StripeConfiguration.ApiKey = "sk_test_51OJhbAEG1zYwB6GkhD5Gcqfp4vtM0m5l8qLgd6fevhjIxoFwcAdC1UGOV8m19PYz0QME0cPuKkcT9dMgsSRaZLew00UcE2FnrK";


                        var domain = "https://smartgroceryappweb20231209093526.azurewebsites.net/";
                        var options = new SessionCreateOptions
                        {
                            SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                            CancelUrl = domain + "customer/cart/index",
                            LineItems = new List<SessionLineItemOptions>(),
                            // {
                            //new SessionLineItemOptions
                            //{
                            //  Price = "price_H5ggYwtDq4fbrJ",
                            //  Quantity = 2,
                            //},
                            // },
                            Mode = "payment",
                        };

                        foreach (var item in ShoppingCartVM.ShoppingCartList)
                        {
                            var SessionLineItem = new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    UnitAmount = (long)(item.Price * 100), //20.50 => 2050
                                    Currency = "usd",
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = item.Product.Title
                                    }
                                },
                                Quantity = item.Count

                            };
                            options.LineItems.Add(SessionLineItem);
                        }
                        var service = new SessionService();
                        Session session = service.Create(options);
                        _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                        _unitOfWork.Save();
                        Response.Headers.Add("Location", session.Url);
                        return new StatusCodeResult(303);


                    }

                }

				return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
			}

            return View(ShoppingCartVM);
           
		}

        public    IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u=>u.Id == id, includeProperties:"ApplicationUser");

            if(orderHeader.PaymentStatus!= SD.PaymentStatusDelayedPayment && orderHeader.PaymentType!="Normal")
            {
                //this is an order by customer
                 var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            EmailSender emailSender = new EmailSender();
             emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - E-Commerce",
              $"<p>New Order Created - {orderHeader.Id}</p>");


            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u=>u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

			_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
			return View(id);
        }



		public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u=>u.Id==cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
            if (cartFromDb.Count <= 1)
            {
                //remove that from cart
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
                
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            
        }


        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked:true);
         
               

            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);


            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }


        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count <= 50) 
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if(shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }

    }
}
