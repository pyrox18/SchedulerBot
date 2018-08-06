using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SchedulerBot.MigrationTool.Documents
{
    public class Permission
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("node")]
        public string Node { get; set; }

        [BsonElement("deniedRoles")]
        public List<string> DeniedRoles { get; set; }

        [BsonElement("deniedUsers")]
        public List<string> DeniedUsers { get; set; }
    }
}
