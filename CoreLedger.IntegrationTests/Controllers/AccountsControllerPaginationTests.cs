using System.Net;
using System.Net.Http.Json;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Models;
using CoreLedger.Domain.Entities;
using CoreLedger.Domain.Enums;
using CoreLedger.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace CoreLedger.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for AccountsController pagination functionality.
/// </summary>
public class AccountsControllerPaginationTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly WebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;

    public AccountsControllerPaginationTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.HttpClient;
    }

    /// <summary>
    /// Gets the count of existing accounts in the database.
    /// Tests will work with existing seeded data from migrations.
    /// </summary>
    private async Task<int> GetAccountCountAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Accounts.CountAsync();
    }

    [Fact]
    public async Task GetAll_WithDefaultPagination_ReturnsFirstPageWithCorrectTotalCount()
    {
        // Arrange
        var expectedTotalCount = await GetAccountCountAsync();

        // Act
        var response = await _client.GetAsync("/api/accounts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        Assert.Equal(expectedTotalCount, result.TotalCount);
        Assert.Equal(100, result.Limit);
        Assert.Equal(0, result.Offset);
        Assert.True(result.Items.Count <= 100);
        Assert.Equal(expectedTotalCount, result.Items.Count); // Should return all items if less than limit
    }

    [Fact]
    public async Task GetAll_WithLimitParameter_ReturnsCorrectNumberOfItems()
    {
        // Arrange
        var totalCount = await GetAccountCountAsync();
        var limit = 10;

        // Act
        var response = await _client.GetAsync($"/api/accounts?limit={limit}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        Assert.Equal(Math.Min(limit, totalCount), result.Items.Count);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(limit, result.Limit);
    }

    [Fact]
    public async Task GetAll_WithOffsetParameter_SkipsCorrectNumberOfItems()
    {
        // Arrange
        var totalCount = await GetAccountCountAsync();
        var offset = 10;
        var limit = 5;

        // Act
        var response = await _client.GetAsync($"/api/accounts?limit={limit}&offset={offset}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        Assert.True(result.Items.Count <= limit);
        Assert.Equal(offset, result.Offset);
        Assert.Equal(totalCount, result.TotalCount);
    }

    [Fact]
    public async Task GetAll_WithLimitExceeding100_CapsAtMaximum()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/accounts?limit=200");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        Assert.Equal(100, result.Limit);
    }

    [Fact]
    public async Task GetAll_SortByCodeAscending_ReturnsSortedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/accounts?limit=10&sortBy=code&sortDirection=asc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        
        var codes = result.Items.Select(a => a.Code).ToList();
        Assert.Equal(codes.OrderBy(c => c).ToList(), codes);
    }

    [Fact]
    public async Task GetAll_SortByCodeDescending_ReturnsSortedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/accounts?limit=10&sortBy=code&sortDirection=desc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        
        var codes = result.Items.Select(a => a.Code).ToList();
        Assert.Equal(codes.OrderByDescending(c => c).ToList(), codes);
    }

    [Fact]
    public async Task GetAll_SortByNameAscending_ReturnsSortedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/accounts?limit=10&sortBy=name&sortDirection=asc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        
        var names = result.Items.Select(a => a.Name).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task GetAll_FilterByActiveStatus_ReturnsOnlyActiveAccounts()
    {
        // Act
        var response = await _client.GetAsync("/api/accounts?filter=status=Active&limit=50");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        
        // If accounts exist, verify filtering works correctly
        if (result.Items.Count > 0)
        {
            Assert.All(result.Items, account => 
                Assert.Equal("Active", account.StatusDescription));
        }
        
        // TotalCount should match filtered results (could be 0 in empty database)
        Assert.True(result.TotalCount >= 0);
    }

    [Fact]
    public async Task GetAll_FilterByInactiveStatus_ReturnsOnlyInactiveAccounts()
    {
        // Act
        var response = await _client.GetAsync("/api/accounts?filter=status=Inactive&limit=50");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        
        // If there are inactive accounts, verify they are all inactive
        if (result.Items.Count > 0)
        {
            Assert.All(result.Items, account => 
                Assert.Equal("Inactive", account.StatusDescription));
        }
    }

    [Fact]
    public async Task GetAll_CombinePaginationSortingAndFiltering_WorksCorrectly()
    {
        // Act - Get second page of active accounts sorted by code descending
        var response = await _client.GetAsync(
            "/api/accounts?limit=10&offset=10&sortBy=code&sortDirection=desc&filter=status=Active");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        
        Assert.Equal(10, result.Offset);
        Assert.True(result.TotalCount >= 0);
        
        // If accounts exist at this offset, verify filtering and sorting
        if (result.Items.Count > 0)
        {
            Assert.All(result.Items, account => 
                Assert.Equal("Active", account.StatusDescription));
            
            var codes = result.Items.Select(a => a.Code).ToList();
            Assert.Equal(codes.OrderByDescending(c => c).ToList(), codes);
        }
    }

    [Fact]
    public async Task GetAll_WithOffsetBeyondTotalCount_ReturnsEmptyResults()
    {
        // Arrange
        var totalCount = await GetAccountCountAsync();
        var offsetBeyondTotal = totalCount + 100;

        // Act
        var response = await _client.GetAsync($"/api/accounts?limit=10&offset={offsetBeyondTotal}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        Assert.NotNull(result);
        
        Assert.Empty(result.Items);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(offsetBeyondTotal, result.Offset);
    }

    [Fact]
    public async Task GetAll_MultipleRequests_ReturnConsistentTotalCount()
    {
        // Arrange
        var expectedTotal = await GetAccountCountAsync();

        // Act - Make multiple requests with different pagination
        var response1 = await _client.GetAsync("/api/accounts?limit=10&offset=0");
        var response2 = await _client.GetAsync("/api/accounts?limit=10&offset=10");
        var response3 = await _client.GetAsync("/api/accounts?limit=10&offset=20");

        // Assert
        var result1 = await response1.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        var result2 = await response2.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        var result3 = await response3.Content.ReadFromJsonAsync<PagedResult<AccountDto>>();
        
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        
        // All requests should return the same total count
        Assert.Equal(expectedTotal, result1.TotalCount);
        Assert.Equal(expectedTotal, result2.TotalCount);
        Assert.Equal(expectedTotal, result3.TotalCount);
        
        // Verify no duplicate IDs across pages
        var allIds = result1.Items.Select(a => a.Id)
            .Concat(result2.Items.Select(a => a.Id))
            .Concat(result3.Items.Select(a => a.Id))
            .ToList();
        
        Assert.Equal(allIds.Count, allIds.Distinct().Count());
    }
}
