using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Services;

namespace Viox.Tests.Spotify;

public class MemoryCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheService<string> _cacheService;

    public MemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new CacheOptions { DefaultAbsoluteExpiration = TimeSpan.FromMinutes(30) });
        _cacheService = new MemoryCacheService<string>(_memoryCache, options, NullLogger<MemoryCacheService<string>>.Instance);
    }

    [Fact]
    public async Task SetAsync_StoresValueInCache()
    {
        const string key = "test-key";
        const string value = "test-value";

        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync(key);

        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsCachedValue()
    {
        const string key = "cached-data";
        const string value = "cached-value";

        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync(key);

        Assert.NotNull(result);
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullForMissingKey()
    {
        var result = await _cacheService.GetAsync("non-existent-key");

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_RemovesExistingKey()
    {
        const string key = "removable-key";
        const string value = "removable-value";

        await _cacheService.SetAsync(key, value);
        var removed = await _cacheService.RemoveAsync(key);

        Assert.True(removed);
        var result = await _cacheService.GetAsync(key);
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveAsync_ReturnsFalseForNonExistentKey()
    {
        var removed = await _cacheService.RemoveAsync("non-existent-key");

        Assert.False(removed);
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsExistingValueWithoutCallingFactory()
    {
        const string key = "factory-key";
        const string existingValue = "existing";
        var factoryCalled = false;

        await _cacheService.SetAsync(key, existingValue);

        var result = await _cacheService.GetOrCreateAsync(
            key,
            async ct =>
            {
                factoryCalled = true;
                await Task.CompletedTask;
                return "new-value";
            });

        Assert.Equal(existingValue, result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public async Task GetOrCreateAsync_CallsFactoryWhenKeyMissing()
    {
        const string key = "missing-key";
        const string factoryValue = "factory-generated";
        var factoryCalled = false;

        var result = await _cacheService.GetOrCreateAsync(
            key,
            async ct =>
            {
                factoryCalled = true;
                await Task.CompletedTask;
                return factoryValue;
            });

        Assert.True(factoryCalled);
        Assert.Equal(factoryValue, result);
    }

    [Fact]
    public async Task GetOrCreateAsync_CachesFactoryResult()
    {
        const string key = "cached-factory-result";
        var callCount = 0;

        var result1 = await _cacheService.GetOrCreateAsync(
            key,
            async ct =>
            {
                callCount++;
                await Task.CompletedTask;
                return "value";
            });

        var result2 = await _cacheService.GetOrCreateAsync(
            key,
            async ct =>
            {
                callCount++;
                await Task.CompletedTask;
                return "other-value";
            });

        Assert.Equal(1, callCount);
        Assert.Equal(result1, result2);
    }
}
