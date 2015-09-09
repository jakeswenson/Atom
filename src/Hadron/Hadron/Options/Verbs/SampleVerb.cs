using System;
using System.Collections.Generic;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Atom.Data;
using Atom.Generation;
using Atom.Data.Serialization;

namespace HadronApplication.Options.Verbs
{
    public class SampleVerb : Verb
    {
        [Option('f', Required = false, HelpText = "Generates a full atom", DefaultValue = true)]
        public bool Full { get; set; }

        protected override IEnumerable<GeneratorResult> Generate()
        {
            if (Full)
            {
                HelpFullGeneric();

                HelpLookup();

                WriteTypes();
            }

            yield break;
        }

        private void WriteTypes()
        {
            var types = AtomRootConverter.TypeFactories.Keys;

            Write("Possible member types", types);
        }

        private void HelpLookup()
        {
            var atom = new AtomModel
            {
                Name = "LookupType",
                Lookup = new LookupDefinition
                {
                    Values = new List<LookupValue>
                    {
                        new LookupValue
                        {
                            Name = "Item1",
                            Description = "Item1 description"
                        },
                        new LookupValue
                        {
                            Name = "Item2",
                            Description = "Item2 description"
                        }
                    }
                }
            };

            Write("Lookup atom", atom);
        }

        private void HelpFullGeneric()
        {
            //atom.AdditionalInfo = null;
            //atom.Groups = null;
            //atom.IsLookup = false;
            //atom.LookupValues = null;
            //atom.Members = new OrderedAtomMembers();

            //Enumerable.Range(0, 2)
            //          .ForEach(i => atom.Members.Add(fixture.Create<AtomMemberInfo>()));

            //Write("Generic atom, no lookup", atom);
        }

        private void Write<T>(string title, T atom)
        {
            Console.WriteLine("---------------");
            Console.WriteLine(title);
            Console.WriteLine();
            Console.WriteLine(JsonConvert.SerializeObject(atom, Formatting.Indented, new JsonSerializerSettings
                                                                                     {
                                                                                         DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                                         ContractResolver = new CamelCasePropertyNamesContractResolver()
                                                                                     }));
            Console.WriteLine();
        }
    }
}
