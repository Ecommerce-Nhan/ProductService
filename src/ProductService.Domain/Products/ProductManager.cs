﻿using SharedLibrary.Exceptions;
using SharedLibrary.Exceptions.Products;
using System.Xml.Linq;

namespace ProductService.Domain.Products;

public class ProductManager
{
    private readonly IProductReadOnlyRepository _repository;
    public ProductManager(IProductReadOnlyRepository repository)
    {
        _repository = repository;
    }
    public async Task<Product> CreateAsync(string name,
                         string code,
                         string? note,
                         float costPrice,
                         float unitPrice,
                         List<string>? images)
    {
        var existingEntity = await _repository.FindByCodeAsync(code);
        if (existingEntity != null)
        {
            throw new ProductExistException(code);
        }
        existingEntity = await _repository.FindByNameAsync(name);
        if (existingEntity != null)
        {
            throw new ProductExistException(name, true);
        }

        return new Product
        {
            Name = name,
            Code = code,
            Note = note,
            CostPrice = costPrice,
            UnitPrice = unitPrice,
            Images = images
        };
    }

    public async Task ChangeCodeAsync(Product product, string newCode)
    {
        if (string.IsNullOrEmpty(newCode) || string.IsNullOrWhiteSpace(newCode))
        {
            throw new BaseException("Code is required");
        }

        var existingEntity = await _repository.FindByCodeAsync(newCode);
        if (existingEntity != null && existingEntity.Id != product.Id)
        {
            throw new ProductExistException(newCode, false);
        }

        product.ChangeCode(newCode);
    }

    public async Task ChangeNameAsync(Product product, string newName)
    {
        if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
        {
            throw new BaseException("Name is required");
        }

        var existingEntity = await _repository.FindByNameAsync(newName);
        if (existingEntity != null && existingEntity.Id != product.Id)
        {
            throw new ProductExistException(newName, true);
        }

        product.ChangeName(newName);
    }
}
