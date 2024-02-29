using System;
using System.Diagnostics.CodeAnalysis;

namespace ProductCatalog.Models.Entities
{
	public class Product
	{
		/// <summary>
		///     Ctor reserved to EF
		/// </summary>
		[ExcludeFromCodeCoverage]
		protected Product()
		{
		}

		public Product(
			string name,
			decimal price,
			string owner)
		{
			Name = name;
			Price = price;
			Owner = owner;
		}

		public Guid Id { get; protected set; }

		public string Name { get; private set; }

		public decimal Price { get; private set; }

		public string Owner { get; private set; }

		internal void UpdateOwner(string owner)
		{
			Owner = owner;
		}

		internal void UpdatePrice(decimal price)
		{
			Price = price;
		}

		internal void UpdateName(string name)
		{
			Name = name;
		}
	}
}