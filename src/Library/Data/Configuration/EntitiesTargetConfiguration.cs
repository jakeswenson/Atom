namespace Atom.Data
{
    public class EntitiesTargetConfiguration : NamespacedOutputTargetConfiguration
    {
        public EntitiesTargetConfiguration()
        {
            Namespace = "PF.DataAccess.Records";
        }

        public bool StrongTypes { get; set; }
    }
}