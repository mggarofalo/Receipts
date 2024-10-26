# C# Development Guidelines

You are an AI assistant to a C# developer. You are tasked with writing high-quality, idiomatic C# code using the latest language features. Please adhere to the following guidelines:

## 1. Explicit Type Declarations
- Use explicit type declarations instead of `var` wherever possible
- Exception: Use `var` when it enhances readability and the type is `object` or an obvious primitive

## 2. Latest C# Features
Utilize the latest C# language features, including:
- Primary constructors
- Simple array initialization
- Collection expressions

## 3. Unit Tests with xUnit
- Write unit tests using xUnit
- Use `expected` and `actual` as variable names for expected and actual values in assertions
- Use Arrange/Act/Assert structure for unit tests
- Avoid testing implementation details

## 4. Code Clarity and Readability
- Ensure the code is clear, concise, and easy to understand
- Follow best practices for naming conventions and code organization

### Good Example 1: Using Primary Constructors and Limited Auto-Properties
```
public class Order(int orderId, List<string> items)
{
    public int OrderId { get; } = orderId;
    public List<string> Items { get; } = items;

    public void AddItem(string item) {
        Items.Add(item);
    }
}
```

### Unit Test for Good Example 1
```
using Xunit;

public class OrderTests
{
    [Fact]
    public void AddItem_ShouldAddItemToOrder()
    {
        // Arrange
        Order order = new Order(1, new List<string>());
        string newItem = "Laptop";
        List<string> expected = new List<string> { "Laptop" };

        // Act
        order.AddItem(newItem);
        List<string> actual = order.Items;

        // Assert
        Assert.Equal(expected, actual);
    }
}
```

### Good Example 2: Explicit Types and Curly Braces
```
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```

### Unit Test for Good Example 2
```
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_ShouldReturnSumOfTwoNumbers()
    {
        // Arrange
        Calculator calculator = new Calculator();
        int expected = 5;

        // Act
        int actual = calculator.Add(2, 3);

        // Assert
        Assert.Equal(expected, actual);
    }
}
```

### Bad Example: Overuse of 'var' and Lack of Latest Features like Primary Constructors
```
public class Product
{
    public var Name { get; set; } // Incorrect use of 'var'
    public var Price { get; set; } // Incorrect use of 'var'

    public Product(var name, var price) // Incorrect use of 'var'
    {
        Name = name;
        Price = price;
    }
}
```

### Unit Test for Bad Example
```
using Xunit;

public class ProductTests
{
    [Fact]
    public void Product_ShouldInitializeCorrectly()
    {
        // Arrange
        var product = new Product("Book", 9.99); // Incorrect use of 'var'
        var expectedName = "Book"; // Incorrect use of 'var'
        var expectedPrice = 9.99; // Incorrect use of 'var'

        // Act
        var resultName = product.Name; // Incorrect use of 'var', result instead of actual
        var resultPrice = product.Price; // Incorrect use of 'var', result instead of actual

        // Assert
        Assert.Equal(expectedName, resultName);
        Assert.Equal(expectedPrice, resultPrice);
		Assert.True(true); // Adding an assertion just to make the test pass
		Assert.NotNull(product); // Asserting something not covered by the test as designed
		Assert.IsType<Product>(product); // Asserting something not covered by the test as designed
    }
}
```

### Bad Example: Lack of `new(..)` syntax
```
var service = new DatabaseMigratorService(_mockServiceProvider.Object); // Incorrect use of 'var', should be 'new(..)'
```

### Good Example: Using `new(..)` syntax
```
DatabaseMigratorService service = new(_mockServiceProvider.Object);
```
