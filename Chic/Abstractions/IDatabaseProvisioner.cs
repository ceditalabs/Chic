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
