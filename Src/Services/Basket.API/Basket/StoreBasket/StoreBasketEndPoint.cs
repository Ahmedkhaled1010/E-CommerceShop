
namespace Basket.API.Basket.StoreBasket
{
    public record StoreBasketRequest(ShoppingCart Cart);
    public record StoreBasketResponse(string UserName);

    public class StoreBasketEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
                app.MapPost("/basket", async (StoreBasketRequest request, ISender sender) =>
                {
                    var command = new StoreBasketCommand(request.Cart);
                    var result = await sender.Send(command);
                    var response = result.Adapt<StoreBasketResponse>();
                    return Results.Created($"/basket/{response.UserName}",response);
                })
                .WithName("CreateBasket")
                .Produces<StoreBasketResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Stores a shopping cart for a user")
                .WithDescription("Stores a shopping cart for a user. The shopping cart is stored in the database and can be retrieved later.");

        }
    }
}
