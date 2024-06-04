using Amazon.Auth.AccessControlPolicy;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using Yah.Hub.Common.Identity;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Domain.Catalog;
using Yah.Hub.Marketplace.MercadoLivre.Application.Mappings.Resolvers;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement;
using Identity = Yah.Hub.Common.Identity.Identity;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Mappings
{
    public class AnnouncementMappingProfile : Profile
    {
        public AnnouncementMappingProfile() 
        {
            this.RegisterAnnouncementToMeliAnnoucement();
            this.RegisterAnnouncementInventoryToMeliAnnoucementInventory();
            this.RegisterAnnouncementPriceToMeliAnnoucementPrice();
            this.RegisterMeliAnnoucementToAnnouncementItem();
        }

        #region Annoucement -> MeliAnnouncement
        private void RegisterAnnouncementToMeliAnnoucement()
        {
            base.CreateMap<Announcement, MeliAnnouncement>()
                .ForMember(dest => dest.ItemId, opt => 
                {
                    opt.PreCondition((src) => !string.IsNullOrEmpty(src.MarketplaceId));
                    opt.ResolveUsing((src) =>
                    {
                        return src.MarketplaceId;
                    });
                })
                .ForMember(dest => dest.ListingType, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Item.ListingTypeId ?? String.Empty;
                    });
                })
                .ForMember(dest => dest.Balance, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Item.Shipping.LogisticType != "fulfillment" ? src.Item.AvailableQuantity : 0;
                    });
                })
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Item.Price;
                    });
                })
                .ForMember(dest => dest.Status, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        switch (src.Item.Status)
                        {
                            case Common.Enums.EntityStatus.Accepted:
                                return AnnouncementStatus.Active;
                            case Common.Enums.EntityStatus.Stopped:
                                return AnnouncementStatus.Paused;
                            case Common.Enums.EntityStatus.Closed:
                                return AnnouncementStatus.Closed;
                            default:
                                return AnnouncementStatus.Active;
                        }
                    });
                })
                .ForMember(dest => dest.Description, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        var description = new Description() { PlainText = src.Product.Description };

                        return description;
                    });
                })
                .ForMember(dest => dest.Currency, opt => opt.UseValue("BRL"))
                .ForMember(dest => dest.BuyingMode, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Item.BuyingMode;
                    });
                })
                .ForMember(dest => dest.Condition, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Item.Condition;
                    });
                })
                .ForMember(dest => dest.Title, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Item.Title;
                    });
                })
                .ForMember(dest => dest.CategoryId, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Item.CategoryId;
                    });
                })
                .ForMember(dest => dest.Channels, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        var channel = "marketplace";
                        var channels = new List<string>();

                        channels.Add(channel);

                        return channels;
                    });
                })
                .ForMember(dest => dest.Shipping, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        var shipping = new MeliShipping();
                        var dimensions = new Dimensions()
                        {
                            Height = double.Parse(src.Item.Dimension.Height),
                            Weight = decimal.Parse(src.Item.Dimension.Weight),
                            Width = double.Parse(src.Item.Dimension.Width),
                            Length = double.Parse(src.Item.Dimension.Length)
                        };

                        if (src.Item.Shipping.FreeShipping)
                        {
                            shipping.ShippingMode = src.Item.Shipping.ShippingMode;
                            shipping.FreeShipping = src.Item.Shipping.FreeShipping;
                            shipping.LogisticType = src.Item.Shipping.LogisticType;
                            shipping.Tags = src.Item.Shipping.Tags;
                            shipping.Dimensions = dimensions;
                            shipping.FreeMethods = new FreeMethod()
                            {
                                Id = 1000009,
                                Rule = new Rule()
                                {
                                    FreeMode = "country",
                                }
                            };

                        }
                        else
                        {
                            shipping.ShippingMode = src.Item.Shipping.ShippingMode;
                            shipping.FreeShipping = src.Item.Shipping.FreeShipping;
                            shipping.LogisticType = src.Item.Shipping.LogisticType;
                            shipping.Tags = src.Item.Shipping.Tags;
                            shipping.Dimensions = dimensions;
                        }

                        return shipping;
                    });
                })
                .ForMember(dest => dest.Variations, opt =>
                {
                    opt.PreCondition((src) => src.Item.Variations != null && src.Item.Variations.Count() > 0);
                    opt.ResolveUsing<VariationResolver>();
                })
                .ForMember(dest => dest.Picture, opt =>
                {
                    opt.PreCondition((src) => src.Item.Pictures != null && src.Item.Pictures.Count() > 0);
                    opt.MapFrom(src => src.Item.Pictures.Select(x => new MeliPicture() { Source = x.Source }).ToList());
                })
                .ForMember(dest => dest.Attributes, opt =>
                {
                    opt.PreCondition((src) => src.Item.Attributes != null && src.Item.Attributes.Count() > 0);
                    opt.ResolveUsing((src) =>
                    {
                        var returnAttributes = new List<MeliAttribute>();

                        foreach(var attribute in src.Item.Attributes)
                        {
                            returnAttributes.Add(new MeliAttribute() { 
                            Id = attribute.Id,
                            Name = attribute.Name,
                            Value = attribute.Value,
                            ValueName = attribute.ValueName,
                            });
                        }

                        return returnAttributes;
                    });
                })
                .ForMember(dest => dest.SaleTerms, opt =>
                {
                    opt.PreCondition((src) => src.Item.SaleTerms != null && src.Item.SaleTerms.Count() > 0);
                    opt.ResolveUsing((src) =>
                    {
                        var returnAttributes = new List<MeliAttribute>();

                        foreach (var attribute in src.Item.SaleTerms)
                        {
                            returnAttributes.Add(new MeliAttribute()
                            {
                                Id = attribute.Id,
                                Name = attribute.Name,
                                Value = attribute.Value,
                                ValueName = attribute.ValueName,
                            });
                        }

                        return returnAttributes;
                    });
                });
            
        }
        #endregion

        #region MeliAnnouncement -> Annoucement

        private void RegisterMeliAnnoucementToAnnouncementItem()
        {
            base.CreateMap<MeliAnnouncement, AnnouncementItem>()
                .ForMember(dest => dest.ListingTypeId, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.ListingType;
                    });
                })
                .ForMember(dest => dest.AvailableQuantity, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Balance;
                    });
                })
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Price;
                    });
                })
                .ForMember(dest => dest.Status, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        switch (src.Status)
                        {
                            case AnnouncementStatus.Active:
                                return Common.Enums.EntityStatus.Accepted;
                            case AnnouncementStatus.Closed:
                                return Common.Enums.EntityStatus.Closed;
                            case AnnouncementStatus.Paused:
                                return Common.Enums.EntityStatus.Stopped;
                            case AnnouncementStatus.UnderReview:
                                if (src.SubStatus.Contains("forbidden"))
                                {
                                    return Common.Enums.EntityStatus.Closed;
                                }

                                return Common.Enums.EntityStatus.PendingValidation;

                            default:
                                return Common.Enums.EntityStatus.Unknown;
                        }
                    });
                })
                .ForMember(dest => dest.SubStatus, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.SubStatus;
                    });
                })
                .ForMember(dest => dest.BuyingMode, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.BuyingMode;
                    });
                })
                .ForMember(dest => dest.Condition, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Condition;
                    });
                })
                .ForMember(dest => dest.Title, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.Title;
                    });
                })
                .ForMember(dest => dest.CategoryId, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        return src.CategoryId;
                    });
                })
                .ForMember(dest => dest.Shipping, opt =>
                {
                    opt.ResolveUsing((src) =>
                    {
                        var shipping = new Shipping();
                        if (src.Shipping.FreeShipping)
                        {
                            shipping.ShippingMode = src.Shipping.ShippingMode;
                            shipping.FreeShipping = src.Shipping.FreeShipping;
                            shipping.LogisticType = src.Shipping.LogisticType;
                            shipping.Tags = src.Shipping.Tags;
                            shipping.FreeMethods = new FreeMethods()
                            {
                                Id = 1000009,
                                Rule = new Rules()
                                {
                                    FreeMode = "country",
                                }
                            };

                        }
                        else
                        {
                            shipping.ShippingMode = src.Shipping.ShippingMode;
                            shipping.FreeShipping = src.Shipping.FreeShipping;
                            shipping.LogisticType = src.Shipping.LogisticType;
                            shipping.Tags = src.Shipping.Tags;
                        }

                        return shipping;
                    });
                })
                .ForMember(dest => dest.Variations, opt =>
                {
                    opt.PreCondition((src) => src.Variations != null && src.Variations.Count() > 0);
                    opt.ResolveUsing((src) =>
                    {
                        var returnVariations = new List<Domain.Announcement.AnnouncementVariation>();

                        foreach (var variation in src.Variations)
                        {
                            returnVariations.Add(new Domain.Announcement.AnnouncementVariation()
                            {
                                Id = variation.Id,
                                AttributesCombinations = variation.AttributesCombinations.Select(x => new Hub.Domain.Announcement.Attribute()
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Value = x.Value,
                                    ValueName = x.ValueName,
                                }).ToList(),
                                Attributes = variation.Attributes.Select(x => new Hub.Domain.Announcement.Attribute()
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    Value = x.Value,
                                    ValueName = x.ValueName,
                                }).ToList(),
                                SellerCustomField = variation.SellerCustomField,
                                Balance = variation.Balance,
                                Sold = variation.Sold,
                                Price = variation.Price,
                                Pictures = variation.Pictures
                            });
                        }
                        return returnVariations;
                    });
                })
                .ForMember(dest => dest.Pictures, opt =>
                {
                    opt.PreCondition((src) => src.Picture != null && src.Picture.Count() > 0);
                    opt.MapFrom(src => src.Picture.Select(x => new Picture() { Source = x.Source }).ToList());
                })
                .ForMember(dest => dest.Attributes, opt =>
                {
                    opt.PreCondition((src) => src.Attributes != null && src.Attributes.Count() > 0);
                    opt.ResolveUsing((src) =>
                    {
                        var returnAttributes = new List<Hub.Domain.Announcement.Attribute>();

                        foreach (var attribute in src.Attributes)
                        {
                            returnAttributes.Add(new Hub.Domain.Announcement.Attribute()
                            {
                                Id = attribute.Id,
                                Name = attribute.Name,
                                Value = attribute.Value,
                                ValueName = attribute.ValueName,
                            });
                        }

                        return returnAttributes;
                    });
                })
                .ForMember(dest => dest.SaleTerms, opt =>
                {
                    opt.PreCondition((src) => src.SaleTerms != null && src.SaleTerms.Count() > 0);
                    opt.ResolveUsing((src) =>
                    {
                        var returnAttributes = new List<Hub.Domain.Announcement.Attribute>();

                        foreach (var attribute in src.SaleTerms)
                        {
                            returnAttributes.Add(new Hub.Domain.Announcement.Attribute()
                            {
                                Id = attribute.Id,
                                Name = attribute.Name,
                                Value = attribute.Value,
                                ValueName = attribute.ValueName,
                            });
                        }

                        return returnAttributes;
                    });
                });
        }

        #endregion

        #region AnnouncementInventory -> MeliAnnoucementInventory

        public void RegisterAnnouncementInventoryToMeliAnnoucementInventory()
        {
            base.CreateMap<AnnouncementInventory, MeliAnnoucementInventory>()
                .ForMember(dest => dest.ItemId, opt =>
                {
                    opt.PreCondition((src) => !string.IsNullOrEmpty(src.Announcement.MarketplaceId));
                    opt.ResolveUsing((src) =>
                    {
                        return src.Announcement.MarketplaceId;
                    });
                })
                .ForMember(dest => dest.Stock, opt =>
                {
                    opt.PreCondition((src) => src.Announcement.Item.Variations == null || !src.Announcement.Item.Variations.Any());
                    opt.ResolveUsing((src) =>
                    {
                        return src.Announcement.Item.Shipping.LogisticType != "fulfillment" ? src.AffectedSku.Inventory.Balance : 0;
                    });
                })
                .ForMember(dest => dest.Variations, opt =>
                {
                    opt.PreCondition((src) => src.Announcement.Item.Variations != null && src.Announcement.Item.Variations.Any());
                    opt.ResolveUsing((src) =>
                    {
                        var variations = new List<AnnoucementInventoryVariation>();

                        foreach (var variation in src.Announcement.Item.Variations)
                        {
                            variations.Add( new AnnoucementInventoryVariation()
                            {
                                Id = variation.Id,
                                Stock = src.Announcement.Item.Shipping.LogisticType != "fulfillment"
                                        ? variation.SellerCustomField == src.AffectedSku.IntegrationId
                                            ? src.AffectedSku.Inventory.Balance 
                                            : variation.Balance ?? 0
                                        : 0
                            });
                        }

                        return variations;
                    });
                });
        }

        #endregion

        #region AnnouncementPrice -> MeliAnnoucementPrice

        public void RegisterAnnouncementPriceToMeliAnnoucementPrice()
        {
            base.CreateMap<AnnouncementPrice, MeliAnnoucementPrice>()
                .ForMember(dest => dest.ItemId, opt => 
                {
                    opt.ResolveUsing((src) => src.Announcement.MarketplaceId);
                })
                .ForMember(dest => dest.Price, opt =>
                {
                    opt.PreCondition((src) => src.Announcement.Item.Variations == null || !src.Announcement.Item.Variations.Any());
                    opt.ResolveUsing((src) =>
                    {
                        return src.AffectedSku.Price.SalePrice.HasValue ? src.AffectedSku.Price.SalePrice : src.AffectedSku.Price.Retail;
                    });
                })
                .ForMember(dest => dest.Variations, opt =>
                {
                    opt.PreCondition((src) => src.Announcement.Item.Variations != null && src.Announcement.Item.Variations.Any());
                    opt.ResolveUsing((src) =>
                    {
                        var variations = new List<AnnoucementPriceVariation>();

                        foreach (var item in src.Announcement.Item.Variations)
                        {
                            var sku = src.Skus.Where(x => x.IntegrationId == item.SellerCustomField).FirstOrDefault();

                            variations.Add( new AnnoucementPriceVariation()
                            {
                                Id = item.Id,
                                Price = sku.Price.SalePrice.HasValue ? sku.Price.SalePrice ?? 0 : sku.Price.Retail
                            });
                        }

                        var maxPrice = variations.Max(x => x.Price);

                        variations.Select(x => { x.Price = maxPrice; return x; });

                        return variations;
                    });
                });
        }

        #endregion
    }
}
