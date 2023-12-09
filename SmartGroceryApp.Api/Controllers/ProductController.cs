using Microsoft.AspNetCore.Mvc;
using SmartGroceryApp.DataAccess.Repository.IRepository;
using SmartGroceryApp.Models;

namespace SmartGroceryApp.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ProductController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;

		private readonly ILogger<ProductController> _logger;

		public ProductController(ILogger<ProductController> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		[HttpGet(Name = "GetProducts")]
		public IEnumerable<Product> GetProducts()
		{
			return _unitOfWork.Product.GetAll();
		}
	}
}