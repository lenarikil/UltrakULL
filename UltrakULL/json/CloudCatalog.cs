using System.Collections.Generic;
using Newtonsoft.Json;

namespace UltrakULL.json
{
    /// <summary>
    /// Wire format of the online translation catalog (catalog.json published on
    /// Cloudflare Pages, schema_version 1). Property names mirror the JSON keys
    /// exactly and every field is nullable so a partially filled catalog never
    /// fails to load.
    ///
    /// Service fields the publishing bot writes for itself (like_emoji,
    /// likes_updated_at, _image_attachment_ids, download_checked_at,
    /// download_check) are intentionally not modeled; the serializer ignores them.
    /// </summary>
    public class CloudCatalogDocument
    {
        [JsonProperty("schema_version")]
        public int? schema_version;

        [JsonProperty("updated_at")]
        public string updated_at;

        [JsonProperty("translations")]
        public List<CloudCatalogTranslation> translations;
    }

    /// <summary>
    /// One translation record of the catalog. Note that <c>authors</c> is a plain
    /// string (not a list) in this format, and that <c>id</c> is the stable
    /// translation identity (a Discord thread snowflake), never a language code.
    /// </summary>
    public class CloudCatalogTranslation
    {
        [JsonProperty("id")]
        public string id;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("language")]
        public string language;

        [JsonProperty("version")]
        public string version;

        [JsonProperty("authors")]
        public string authors;

        [JsonProperty("description")]
        public string description;

        [JsonProperty("download")]
        public string download;

        [JsonProperty("features")]
        public CloudCatalogFeatures features;

        [JsonProperty("logo")]
        public string logo;

        [JsonProperty("gallery")]
        public List<string> gallery;

        [JsonProperty("likes")]
        public int? likes;

        [JsonProperty("discord")]
        public CloudCatalogDiscord discord;

        [JsonProperty("updated_at")]
        public string updated_at;

        [JsonProperty("social_links")]
        public List<List<string>> social_links;
    }

    /// <summary>
    /// Capability flags of a translation. Values are "None" / "Partial" / "Full"
    /// strings; anything else (including missing values) maps to Unknown.
    /// </summary>
    public class CloudCatalogFeatures
    {
        [JsonProperty("voice_over")]
        public string voice_over;

        [JsonProperty("additional_voice_over")]
        public string additional_voice_over;

        [JsonProperty("replacement_textures")]
        public string replacement_textures;

        [JsonProperty("custom_fonts")]
        public string custom_fonts;
    }

    /// <summary>Discord source thread coordinates (all ids are snowflake strings).</summary>
    public class CloudCatalogDiscord
    {
        [JsonProperty("guild_id")]
        public string guild_id;

        [JsonProperty("forum_id")]
        public string forum_id;

        [JsonProperty("thread_id")]
        public string thread_id;

        [JsonProperty("starter_message_id")]
        public string starter_message_id;
    }
}
