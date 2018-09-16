using Chic.Abstractions;
using Chic.DependencyInjection;
using Chic.Internal;
using Chic.Tests.Models;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Chic.Tests
{
    public class InternalTypeTableMappingTests : TestBase
    {
        [Fact]
        public void CanDetectTypeTableForSampleModel()
        {
            var typeMap = TypeTableMaps.Get<Models.SampleEntity>();

            Assert.Equal("SampleEntities", typeMap.TableName);

            Assert.Contains(typeMap.Columns, m => m.Name == nameof(SampleEntity.Id) && m.Type == typeof(int) && m.DbType == SqlDbType.Int);
            Assert.Contains(typeMap.Columns, m => m.Name == nameof(SampleEntity.Name) && m.Type == typeof(string) && m.DbType == SqlDbType.NVarChar);
            Assert.Contains(typeMap.Columns, m => m.Name == nameof(SampleEntity.Description));
        }
    }
}
