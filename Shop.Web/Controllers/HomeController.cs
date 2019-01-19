using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shop.Cqrs.Queries;
using Shop.DataProjections.Queries;
using Shop.Web.Models;

namespace Shop.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQueryService _queryService;
        private readonly IMapper _mapper;

        public HomeController(
            IQueryService queryService, 
            IMapper mapper)
        {
            _queryService = queryService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var products = await _queryService.QueryAsync(new GetProducts(), cancellationToken);

            var cartItems = await _queryService.QueryAsync(new GetCartItems(HttpContext.Session.Id), cancellationToken);

            var model = products.GroupJoin(
                    cartItems,
                    product => product.ProductId,
                    cartItem => cartItem.ProductId,
                    (p, ci) => new
                    {
                        Product = p,
                        CartItems = ci
                    })
                .SelectMany(
                    productCartItems => productCartItems.CartItems.DefaultIfEmpty(),
                    (product, cartItem) =>
                    {
                        var productViewModel = _mapper.Map<ProductViewModel>(product.Product);
                        productViewModel.CountInCart = cartItem?.Count ?? 0;

                        return productViewModel;
                    });

            return View(model);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
