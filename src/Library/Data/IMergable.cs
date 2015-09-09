namespace Atom.Data
{
    public interface IMergable<in T>
    {
        void Merge(T defaults);
    }
}
