namespace GHI_ASSET_CARGO.Domain.Entities
{
    /// <summary>
    /// Stores soft-deleted entity data to preserve historical records without permanently removing data.
    /// </summary>
    public class DeletedEntity : BaseEntity
    {
        /// <summary>Type of entity that was deleted (e.g., "Shipment", "Airline", "Financial", "Document").</summary>
        public string EntityType { get; set; }

        /// <summary>ID of the original deleted entity.</summary>
        public Guid EntityId { get; set; }

        /// <summary>JSON serialization of the deleted entity data.</summary>
        public string EntityData { get; set; }

        /// <summary>ID of the user who performed the deletion.</summary>
        public Guid? DeletedByUserId { get; set; }

        /// <summary>Email of the user who performed the deletion.</summary>
        public string DeletedByEmail { get; set; }

        /// <summary>Full name of the user who performed the deletion.</summary>
        public string DeletedByName { get; set; }

        /// <summary>Timestamp when the entity was deleted.</summary>
        public DateTimeOffset DeletedDate { get; set; }

        /// <summary>IP address from which the deletion was made.</summary>
        public string IpAddress { get; set; }

        /// <summary>Optional reason for deletion.</summary>
        public string DeletionReason { get; set; }
    }
}
