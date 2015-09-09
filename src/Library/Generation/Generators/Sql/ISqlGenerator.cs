using Atom.Generation.Data;

namespace Atom.Generation
{
    public interface ISqlGenerator<out TResult>
    {
        TResult Generate();
    }
}
