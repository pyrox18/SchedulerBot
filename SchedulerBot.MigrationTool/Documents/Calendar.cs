using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SchedulerBot.MigrationTool.Documents
{
    [BsonNoId]
    public class Calendar
    {
        [BsonElement("_id")]
        public string Id { get; set; }

        [BsonElement("__v")]
        public int Version { get; set; }

        [BsonElement("defaultChannel")]
        public string DefaultChannel { get; set; }

        [BsonElement("prefix")]
        public string Prefix { get; set; }

        [BsonElement("timezone")]
        public string Timezone { get; set; }

        [BsonElement("events")]
        public List<Event> Events { get; set; }

        [BsonElement("permissions")]
        public List<Permission> Permissions { get; set; }
    }
}
