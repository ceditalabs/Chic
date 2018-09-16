// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System.ComponentModel.DataAnnotations.Schema;

namespace Chic.Tests.Models
{
    [Table("ConventionEntities")]
    public class ConventionEntity
    {
        public int ConventionEntityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
