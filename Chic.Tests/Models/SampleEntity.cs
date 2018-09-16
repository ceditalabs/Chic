using Chic.Constraints;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chic.Tests.Models
{
    [Table("SampleEntities")]
    public class SampleEntity : IKeyedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
