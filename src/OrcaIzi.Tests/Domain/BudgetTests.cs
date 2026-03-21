﻿namespace OrcaIzi.Tests.Domain
{
    public class BudgetTests
    {
        [Fact]
        public void AddItem_ShouldCalculateTotalCorrectly()
        {
            // Arrange
            var budget = new Budget("Title", "Desc", Guid.NewGuid(), DateTime.UtcNow.AddDays(10));

            // Act
            budget.AddItem("Item 1", "Desc 1", 2, 50.0m); // 100
            budget.AddItem("Item 2", "Desc 2", 1, 200.0m); // 200

            // Assert
            budget.TotalAmount.Should().Be(300.0m);
            budget.Items.Should().HaveCount(2);
        }

        [Fact]
        public void RemoveItem_ShouldRecalculateTotal()
        {
            // Arrange
            var budget = new Budget("Title", "Desc", Guid.NewGuid(), DateTime.UtcNow.AddDays(10));
            budget.AddItem("Item 1", "Desc 1", 2, 50.0m); // 100
            var item = budget.Items.First();

            // Act
            budget.RemoveItem(item.Id);

            // Assert
            budget.TotalAmount.Should().Be(0);
            budget.Items.Should().BeEmpty();
        }
    }
}



