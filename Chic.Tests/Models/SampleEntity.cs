using Chic.Constraints;

namespace Chic.Tests.Models
{
    public class SampleEntity : IKeyedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
