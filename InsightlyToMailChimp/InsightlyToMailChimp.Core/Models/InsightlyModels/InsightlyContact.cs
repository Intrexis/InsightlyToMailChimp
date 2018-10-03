using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace InsightlyToMailChimp.Core.Models.InsightlyModels
{
    public class InsightlyContact
    {
        private const string BranchCustomFieldId = "CONTACT_FIELD_3";

        [JsonProperty("CONTACT_ID")]
        public int ContactId { get; set; }

        [JsonProperty("FIRST_NAME")]
        public string FirstName { get; set; }

        [JsonProperty("LAST_NAME")]
        public string LastName { get; set; }

        [JsonProperty("EMAIL_ADDRESS")]
        public string EmailAddress { get; set; }

        [JsonProperty("CUSTOMFIELDS")]
        public List<CustomFields> CustomFields { get; set; }

        [NotMapped]
        public string Branch => CustomFields?.FirstOrDefault(cf => cf.CustomFieldId == BranchCustomFieldId)?.FieldValue;
    }
}