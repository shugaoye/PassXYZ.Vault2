﻿using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using KPCLib;
using PassXYZ.Vault.Services;
using PassXYZ.Vault.ViewModels;
using System.Collections.ObjectModel;

namespace PassXYZ.Vault.Tests.ViewModels
{
    public class ItemDetailViewModelTests
    {
        ILogger<ItemDetailViewModel> logger;
        readonly IDataStore<Item> dataStore;

        public ItemDetailViewModelTests() 
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole()
                .SetMinimumLevel(LogLevel.Debug));
            logger = loggerFactory.CreateLogger<ItemDetailViewModel>();
            dataStore = new MockDataStore();
        }

        [Fact]
        public async void LoadItemIdTests()
        {
            // Arrange
            ItemDetailViewModel vm = new(dataStore, logger);
            var items = await dataStore.GetItemsAsync(true);
            foreach (var item in items)
            {
                // Act
                await vm.LoadItemId(item.Id);
                // Assert
                Assert.Equal(item.Name, vm.Title);
            }
        }

        [Fact]
        public async void LoadItemIdFailureTest() 
        {
            ItemDetailViewModel vm = new(dataStore, logger);
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => vm.LoadItemId(null));
            Assert.Equal("Value cannot be null. (Parameter 'itemId')", ex.Message);
        }
        [Fact]
        public async void LoadItemWithWrongIdTest() 
        {
            ItemDetailViewModel vm = new(dataStore, logger);
            string uuid = Guid.NewGuid().ToString();
            var ex = await Assert.ThrowsAsync<NullReferenceException>(() => vm.LoadItemId(uuid));
            logger.LogDebug(ex.Message);
            Assert.Equal(uuid, ex.Message);
        }
    }
}