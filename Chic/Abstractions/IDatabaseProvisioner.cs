// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System.Reflection;

namespace Chic.Abstractions
{
    public interface IDatabaseProvisioner
    {
        void AddStepsFromAssemblyResources(Assembly assembly);

        void AddStepFromFile(string fileName);

        void AddStep(object step);

        bool Provision();
    }
}
