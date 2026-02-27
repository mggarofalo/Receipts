namespace Domain.Core;

public class ItemTemplate
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string? DefaultCategory { get; set; }
	public string? DefaultSubcategory { get; set; }
	public Money? DefaultUnitPrice { get; set; }
	public string? DefaultPricingMode { get; set; }
	public string? DefaultItemCode { get; set; }
	public string? Description { get; set; }

	public const string NameCannotBeEmpty = "Name cannot be empty";
	public const string DefaultPricingModeInvalid = "Default pricing mode must be 'quantity' or 'flat'";

	public ItemTemplate(Guid id, string name, string? defaultCategory = null, string? defaultSubcategory = null, Money? defaultUnitPrice = null, string? defaultPricingMode = null, string? defaultItemCode = null, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(NameCannotBeEmpty, nameof(name));
		}

		if (defaultPricingMode is not null and not "quantity" and not "flat")
		{
			throw new ArgumentException(DefaultPricingModeInvalid, nameof(defaultPricingMode));
		}

		Id = id;
		Name = name;
		DefaultCategory = defaultCategory;
		DefaultSubcategory = defaultSubcategory;
		DefaultUnitPrice = defaultUnitPrice;
		DefaultPricingMode = defaultPricingMode;
		DefaultItemCode = defaultItemCode;
		Description = description;
	}
}
