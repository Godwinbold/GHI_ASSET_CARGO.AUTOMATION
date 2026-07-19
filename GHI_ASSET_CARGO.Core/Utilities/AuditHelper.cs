using System.Text.Json;

namespace GHI_ASSET_CARGO.Core.Utilities
{
    /// <summary>
    /// Helper utility for tracking entity changes and creating human-readable audit trails
    /// </summary>
    public static class AuditHelper
    {
        /// <summary>
        /// Creates a human-readable change summary from before and after values
        /// </summary>
        public static string GetChangeSummary(Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues)
        {
            var changes = new List<string>();

            foreach (var key in newValues.Keys)
            {
                if (!oldValues.ContainsKey(key))
                {
                    changes.Add($"{key}: added with value '{newValues[key]}'");
                    continue;
                }

                var oldValue = oldValues[key];
                var newValue = newValues[key];

                if (!Equals(oldValue, newValue))
                {
                    changes.Add($"{key}: changed from '{oldValue}' to '{newValue}'");
                }
            }

            // Check for deleted fields
            foreach (var key in oldValues.Keys)
            {
                if (!newValues.ContainsKey(key))
                {
                    changes.Add($"{key}: removed (was '{oldValues[key]}')");
                }
            }

            return changes.Any() ? string.Join("; ", changes) : "No changes";
        }

        /// <summary>
        /// Serializes a dictionary to JSON for storage
        /// </summary>
        public static string SerializeToJson(Dictionary<string, object?> values)
        {
            try
            {
                return JsonSerializer.Serialize(values);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Creates a dictionary of changed properties
        /// Example: new { Entity = entity, ChangedFields = new[] { nameof(Shipment.Status) } }
        /// </summary>
        public static Dictionary<string, object?> GetChangedFields(object entity, string[] changedFields)
        {
            var result = new Dictionary<string, object?>();
            var properties = entity.GetType().GetProperties();

            foreach (var field in changedFields)
            {
                var property = properties.FirstOrDefault(p => p.Name == field);
                if (property != null)
                {
                    result[field] = property.GetValue(entity);
                }
            }

            return result;
        }
    }
}
