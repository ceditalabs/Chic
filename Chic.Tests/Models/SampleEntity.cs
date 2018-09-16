// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
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
