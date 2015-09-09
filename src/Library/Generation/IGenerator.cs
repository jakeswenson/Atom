using Atom.Data;

namespace Atom.Generation
{
    public interface IGenerator<out T, TConfig>
        where T : GeneratorResult
        where TConfig : TargetConfig
    {
        T Generate(GeneratorArguments<TConfig> generatorArguments);
    }
}
