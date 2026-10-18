
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.API.Data
{

    // This class is a decorator for the IBasketRepository interface that adds caching functionality to the underlying repository.
    // PROXY DESIGN PATTERN  
    // Using the proxy design pattern, this class acts as a proxy for the IBasketRepository interface, allowing it to intercept calls to the underlying repository and add caching functionality.
    public class CachedBasketRepository(IBasketRepository basketRepository, IDistributedCache distributedCache)
        : IBasketRepository

    {


        public async Task<ShoppingCart> GetBasketAsync(string userName, CancellationToken cancellationToken)
        {
            var cachedBasket = await distributedCache.GetStringAsync(userName, cancellationToken);
            if (!string.IsNullOrEmpty(cachedBasket))
                return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket)!;
            var basket = await basketRepository.GetBasketAsync(userName, cancellationToken);
            await distributedCache.SetStringAsync(userName, JsonSerializer.Serialize(basket), cancellationToken);
            return basket;
        }

        public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken)
        {
            await basketRepository.StoreBasketAsync(basket, cancellationToken);
            await distributedCache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cancellationToken);
            return basket;
        }
        public async Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken)
        {

            await basketRepository.DeleteBasketAsync(userName, cancellationToken);
            await distributedCache.RemoveAsync(userName, cancellationToken);
            return true;
        }
    }
}
