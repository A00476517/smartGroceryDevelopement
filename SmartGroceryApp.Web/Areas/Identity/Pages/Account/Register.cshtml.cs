﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using SmartGroceryApp.DataAccess.Repository;
using SmartGroceryApp.DataAccess.Repository.IRepository;
using SmartGroceryApp.Models;
using SmartGroceryApp.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace SmartGroceryApp.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IUnitOfWork unitOfWork
            )
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }


            [Required]
            [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only Alphabets Allowed")]
            public string Name { get; set; }
            [Required]
            [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only Alphabets Allowed")]
            public string? StreetAddress { get; set; }
            [Required]
            [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only Alphabets Allowed")]
            public string? City { get; set; }
            [Required]
            [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only Alphabets Allowed")]
            [Display(Name = "Province")]
            public string? State { get; set; }
            [Required]
            public string? PostalCode { get; set; }
            [Required]
            public string? PhoneNumber { get; set; }
 
            public int? CompanyId { get; set; }
            [Required]
            public int? CountryId { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> CompanyList { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> CountryList { get; set; }



        }



        public async Task OnGetAsync(string returnUrl = null)
        {
            //if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            //{
            //     _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
            //    _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            //    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            //    _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

            //}

            Input = new()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                }),

                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CountryList = _unitOfWork.Country.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })





            };


            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {

            Input.CountryList = _unitOfWork.Country.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            var countryName = Input.CountryList.Where(x => x.Value == Input.CountryId.ToString()).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (!IsValidPostCode(Input))
                {
                    if(countryName.Text=="US")
                    {
                        ModelState.AddModelError("Input.PostalCode", "The  postcode appears to be invalid for USA");
                        return Page();
                    }
                    else if(countryName.Text== "Canada")
                    {
                        ModelState.AddModelError("Input.PostalCode", "The  postcode appears to be invalid for Canada");
                        return Page();
                    }

                }

            }

            if (ModelState.IsValid)
            {
                if (!IsValidPhoneNumber(Input))
                {
                    if (countryName.Text == "US")
                    {
                        ModelState.AddModelError("Input.PhoneNumber", "The  PhoneNumber appears to be invalid for USA");
                        return Page();
                    }
                    else if (countryName.Text == "Canada")
                    {
                        ModelState.AddModelError("Input.PhoneNumber", "The  PhoneNumber appears to be invalid for Canada");
                        return Page();
                    }

                }

            }

            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                user.StreetAddress = Input.StreetAddress;
                user.City = Input.City;
                user.Name = Input.Name;
                user.State = Input.State;
                user.PostalCode = Input.PostalCode;
                user.PhoneNumber = Input.PhoneNumber;

                if (Input.Role == SD.Role_Company)
                {
                    user.CompanyId = Input.CompanyId;
                }


                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!String.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);
                    EmailSender emailSender = new EmailSender();
                    await emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                       $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        if (User.IsInRole(SD.Role_Admin))
                        {

                            TempData["success"] = "New User Created SuccessFully";

                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                        }


                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
          
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }

private static IDictionary<string, string> countryPostCodeRegex = new Dictionary<string, string>
        {
            { "US", "^[0-9]{5}(?:-[0-9]{4})?$" },
            { "Canada", "[ABCEGHJKLMNPRSTVXY][0-9][ABCEGHJKLMNPRSTVWXYZ] ?[0-9][ABCEGHJKLMNPRSTVWXYZ][0-9]" },
        };

        private static IDictionary<string, string> countryPhoneNumberRegex = new Dictionary<string, string>
        {
            { "US", "^[2-9]{3}-[0-9]{3}-[0-9]{4}$" },
            { "Canada", "^([2-9]{1}[0-9]{2})(([2-9]{1})(1[0,2-9]{1}|[0,2-9]{1}[0-9]{1}))([0-9]{4})$" },
        };

        private bool IsValidPostCode(InputModel viewModel)
        {
            var countryName = Input.CountryList.Where(x => x.Value == Input.CountryId.ToString()).FirstOrDefault();
            var regexString = countryPostCodeRegex[countryName.Text];
            var regexMatch = Regex.Match(viewModel.PostalCode, regexString, RegexOptions.IgnoreCase);
            return regexMatch.Success;
        }

        private bool IsValidPhoneNumber(InputModel viewModel)
        {
            var countryName = Input.CountryList.Where(x => x.Value == Input.CountryId.ToString()).FirstOrDefault();
            var regexString = countryPostCodeRegex[countryName.Text];
            var regexMatch = Regex.Match(viewModel.PostalCode, regexString, RegexOptions.IgnoreCase);
            return regexMatch.Success;
        }

    }
}
