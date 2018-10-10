using Newtonsoft.Json;

namespace InsightlyToMailChimp.Core.Models.InsightlyModels
{
    public class CustomFields
    {
        [JsonProperty("FIELD_NAME")]
        public string CustomFieldId { get; set; }

        [JsonProperty("FIELD_VALUE")]
        public string FieldValue { get; set; }
    }
}