using Newtonsoft.Json;

namespace InsightlyToMailChimp.Core.Models.InsightlyModels
{
    public class CustomFields
    {
        [JsonProperty("CUSTOM_FIELD_ID")]
        public string CustomFieldId { get; set; }

        [JsonProperty("FIELD_VALUE")]
        public string FieldValue { get; set; }
    }
}