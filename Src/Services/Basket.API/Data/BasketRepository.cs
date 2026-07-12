
namespace Basket.API.Data
{
    public class BasketRepository(IDocumentSession session) : IBasketRepository
    {
        public async Task<ShoppingCart> GetBasketAsync(string userName, CancellationToken cancellationToken)
        {
          var basket =await session.LoadAsync<ShoppingCart>(userName);
            return basket is null? throw new BasketNotFoundException(userName) :  basket;
        }

        public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken)
        {
            session.Store(basket);
            await session.SaveChangesAsync();
            return basket;
        }
        public async Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken)
        {
            session.Delete<ShoppingCart>(userName);
            await session.SaveChangesAsync();
            return true;
        }

       
    }
}
