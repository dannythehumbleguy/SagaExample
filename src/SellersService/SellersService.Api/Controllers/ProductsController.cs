using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SellersService.Api.Common;
using SellersService.Api.Common.Auth;
using SellersService.Api.Common.Pagination;
using SellersService.Api.Models;
using SellersService.Api.Repositories;

namespace SellersService.Api.Controllers;


[Route("api/[controller]")]
public class ProductsController(ProductRepository productRepository) : AbstractController
{
    /// <summary>
    /// Returns products for buyers
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<Paged<ProductDto>>, BadRequest<Error>>> GetProducts([FromQuery] PaginationRequest form) => 
        await Wrap(productRepository.GetProducts(form));
    
    /// <summary>
    /// Returns sellers products
    /// </summary>
    [ValidateToken]
    [HttpGet("seller")]
    public async Task<Results<Ok<Paged<ProductDto>>, BadRequest<Error>>> GetSellerProducts([FromQuery] PaginationRequest form) => 
        await Wrap(productRepository.GetProducts(UserId, form));
    
    /// <summary>
    /// Creates a product
    /// </summary>
    [HttpPost]
    [ValidateToken]
    public async Task<Results<Ok<Guid>, BadRequest<Error>>> CreateProduct([FromBody] CreateProductRequest request) => 
        await Wrap(productRepository.CreateProduct(UserId, request));
    
    /// <summary>
    /// Updates a product
    /// </summary>
    [HttpPatch]
    [ValidateToken]
    public async Task<Results<Ok<Guid>, BadRequest<Error>>> UpdateProduct([FromBody] UpdateProductRequest request) => 
        await Wrap(productRepository.UpdateProduct(UserId, request));
    
    /// <summary>
    /// Deletes a product
    /// </summary>
    [ValidateToken]
    [HttpDelete("{productId}")]
    public async Task<Results<Ok<Guid>, BadRequest<Error>>> DeleteProduct([FromRoute] Guid productId) => 
        await Wrap(productRepository.DeleteProduct(UserId, productId));
}