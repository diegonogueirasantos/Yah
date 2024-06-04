using AutoMapper;
using Yah.Hub.Domain.Announcement;
using Yah.Hub.Marketplace.MercadoLivre.Application.Models.Announcement;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Mappings.Resolvers
{
    public class VariationResolver : IValueResolver<Announcement, MeliAnnouncement, List<MeliVariation>>
    {
        public List<MeliVariation> Resolve(Announcement source, MeliAnnouncement destination, List<MeliVariation> destMember, ResolutionContext context)
        {
            var returnVariations = new List<MeliVariation>();

            foreach(var variation in source.Item.Variations)
            {
                returnVariations.Add(new MeliVariation()
                {
                    Id = variation.Id,
                    AttributesCombinations = variation.AttributesCombinations.Select(x => new MeliAttribute()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Value = x.Value,
                        ValueName = x.ValueName,
                    }).ToList(),
                    Attributes = variation.Attributes.Select(x => new MeliAttribute() {
                        Id = x.Id,
                        Name = x.Name,
                        Value = x.Value,
                        ValueName = x.ValueName,
                    }).ToList(),
                    SellerCustomField = variation.SellerCustomField,
                    Balance =  source.Item.Shipping.LogisticType != "fulfillment" ? variation.Balance : 0,
                    Sold = variation.Sold,
                    Price = variation.Price,
                    Pictures = variation.Pictures
                });
            }

            return returnVariations;
        }
    }
}
