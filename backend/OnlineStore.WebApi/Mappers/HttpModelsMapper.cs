﻿using OnlineStore.Domain.Entities;
using OnlineStore.Models.Responses;
using OnlineStore.Models.Shared;

namespace OnlineStore.WebApi.Mappers;

public class HttpModelsMapper
{
    public virtual ProductResponse MapProductModel(Product obj)
        => new(obj.Id, obj.Name, obj.Price, obj.Image, obj.Description, obj.CategoryId);

    public virtual CartItemResponse MapCartItemModel(CartItem obj)
        => new(obj.Id, obj.ProductId, obj.Quantity);

    public virtual OrderItemDto MapOrderItemModel(OrderItem obj)
        => new(obj.ProductId, obj.Quantity, obj.Price);
    
    public virtual OrderItem MapOrderItemModel(OrderItemDto obj)
        => new(Guid.Empty, obj.ProductId, obj.Quantity, obj.Price);
    
    public virtual CategoryResponse MapCategoryModel(Category obj)
        => new(obj.ParentId,obj.Id, obj.Name);

    public virtual ParentCategoryResponse MapParentCategoryModel(ParentCategory obj)
        => new(obj.Id, obj.Name);
}