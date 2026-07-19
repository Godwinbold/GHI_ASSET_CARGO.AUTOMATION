using GHI_ASSET_CARGO.Domain.Entities;
using System.Text.Json;

namespace GHI_ASSET_CARGO.Core.Utilities
{
    /// <summary>
    /// Utility class for soft delete operations and deleted entity tracking.
    /// </summary>
    public static class SoftDeleteHelper
    {
        /// <summary>
        /// Records a deleted entity for audit/restoration purposes.
        /// </summary>
        public static DeletedEntity CreateDeletedEntity<T>(
            T deletedEntity,
            Guid? deletedByUserId,
            string deletedByEmail,
            string deletedByName,
            string ipAddress,
            string deletionReason = null) where T : BaseEntity
        {
            var entityJson = JsonSerializer.Serialize(deletedEntity, new JsonSerializerOptions { WriteIndented = false });

            return new DeletedEntity
            {
                EntityType = typeof(T).Name,
                EntityId = deletedEntity.Id,
                EntityData = entityJson,
                DeletedByUserId = deletedByUserId,
                DeletedByEmail = deletedByEmail ?? "unknown@email.com",
                DeletedByName = deletedByName ?? "Unknown",
                DeletedDate = DateTimeOffset.UtcNow,
                IpAddress = ipAddress ?? "",
                DeletionReason = deletionReason
            };
        }
    }
}
