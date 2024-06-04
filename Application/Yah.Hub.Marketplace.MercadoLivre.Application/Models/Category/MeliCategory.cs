using System;
using Newtonsoft.Json;

namespace Yah.Hub.Marketplace.MercadoLivre.Application.Models.Category
{
    public class MeliCategory
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("picture")]
        public string picture { get; set; }

        [JsonProperty("permalink")]
        public string permalink { get; set; }

        [JsonProperty("total_items_in_this_category")]
        public int total_items_in_this_category { get; set; }

        [JsonProperty("path_from_root")]
        public List<PathFromRoot> path_from_root { get; set; }

        [JsonProperty("children_categories")]
        public List<ChildrenCategory> children_categories { get; set; }

        [JsonProperty("attribute_types")]
        public string attribute_types { get; set; }

        [JsonProperty("settings")]
        public Settings settings { get; set; }

        [JsonProperty("channels_settings")]
        public List<ChannelsSetting> channels_settings { get; set; }

        [JsonProperty("meta_categ_id")]
        public object meta_categ_id { get; set; }

        [JsonProperty("attributable")]
        public bool attributable { get; set; }

        [JsonProperty("date_created")]
        public DateTime date_created { get; set; }
    }

    public class ChannelsSetting
    {
        [JsonProperty("channel")]
        public string channel { get; set; }

        [JsonProperty("settings")]
        public Settings settings { get; set; }
    }

    public class ChildrenCategory
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("total_items_in_this_category")]
        public int total_items_in_this_category { get; set; }
    }

    public class PathFromRoot
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }
   
    public class Settings
    {
        [JsonProperty("adult_content")]
        public bool adult_content { get; set; }

        [JsonProperty("buying_allowed")]
        public bool buying_allowed { get; set; }

        [JsonProperty("buying_modes")]
        public List<string> buying_modes { get; set; }

        [JsonProperty("catalog_domain")]
        public string catalog_domain { get; set; }

        [JsonProperty("coverage_areas")]
        public string coverage_areas { get; set; }

        [JsonProperty("currencies")]
        public List<string> currencies { get; set; }

        [JsonProperty("fragile")]
        public bool fragile { get; set; }

        [JsonProperty("immediate_payment")]
        public string immediate_payment { get; set; }

        [JsonProperty("item_conditions")]
        public List<string> item_conditions { get; set; }

        [JsonProperty("items_reviews_allowed")]
        public bool items_reviews_allowed { get; set; }

        [JsonProperty("listing_allowed")]
        public bool listing_allowed { get; set; }

        [JsonProperty("max_description_length")]
        public int max_description_length { get; set; }

        [JsonProperty("max_pictures_per_item")]
        public int max_pictures_per_item { get; set; }

        [JsonProperty("max_pictures_per_item_var")]
        public int max_pictures_per_item_var { get; set; }

        [JsonProperty("max_sub_title_length")]
        public int max_sub_title_length { get; set; }

        [JsonProperty("max_title_length")]
        public int max_title_length { get; set; }

        [JsonProperty("max_variations_allowed")]
        public int max_variations_allowed { get; set; }

        [JsonProperty("maximum_price")]
        public object maximum_price { get; set; }

        [JsonProperty("maximum_price_currency")]
        public string maximum_price_currency { get; set; }

        [JsonProperty("minimum_price")]
        public int? minimum_price { get; set; }

        [JsonProperty("minimum_price_currency")]
        public string minimum_price_currency { get; set; }

        [JsonProperty("mirror_category")]
        public object mirror_category { get; set; }

        [JsonProperty("mirror_master_category")]
        public object mirror_master_category { get; set; }

        [JsonProperty("mirror_slave_categories")]
        public List<object> mirror_slave_categories { get; set; }

        [JsonProperty("price")]
        public string price { get; set; }

        [JsonProperty("reservation_allowed")]
        public string reservation_allowed { get; set; }

        [JsonProperty("restrictions")]
        public List<object> restrictions { get; set; }

        [JsonProperty("rounded_address")]
        public bool rounded_address { get; set; }

        [JsonProperty("seller_contact")]
        public string seller_contact { get; set; }

        [JsonProperty("shipping_options")]
        public List<string> shipping_options { get; set; }

        [JsonProperty("shipping_profile")]
        public string shipping_profile { get; set; }

        [JsonProperty("show_contact_information")]
        public bool show_contact_information { get; set; }

        [JsonProperty("simple_shipping")]
        public string simple_shipping { get; set; }

        [JsonProperty("stock")]
        public string stock { get; set; }

        [JsonProperty("sub_vertical")]
        public object sub_vertical { get; set; }

        [JsonProperty("subscribable")]
        public bool subscribable { get; set; }

        [JsonProperty("tags")]
        public List<object> tags { get; set; }

        [JsonProperty("vertical")]
        public object vertical { get; set; }

        [JsonProperty("vip_subdomain")]
        public string vip_subdomain { get; set; }

        [JsonProperty("buyer_protection_programs")]
        public List<string> buyer_protection_programs { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }
    }
}