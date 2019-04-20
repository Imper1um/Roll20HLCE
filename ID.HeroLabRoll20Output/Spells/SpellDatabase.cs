using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;

namespace ID.HeroLabRoll20Output.Spells
{
    public class SpellDatabase
    {
        private static SpellDatabase _instance;

        public static SpellDatabase Instance => _instance ?? (_instance = new SpellDatabase());

        public SpellSource First(Func<SpellSource, bool> search)
        {
            return _spellSource.First(search);
        }

        public SpellSource FirstOrDefault(Func<SpellSource, bool> search)
        {
            return _spellSource.FirstOrDefault(search);
        }

        public IEnumerable<SpellSource> Where(Func<SpellSource, bool> search)
        {
            return _spellSource.Where(search);
        }

        private SpellDatabase()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ID.HeroLabRoll20Output.spelldb.csv"))
            using (var streamReader = new StreamReader(stream))
            using (var csvReader = new CsvReader(streamReader))
            {
                csvReader.Read();
                csvReader.ReadHeader();
                while (csvReader.Read())
                {
                    _spellSource.Add(new SpellSource(csvReader));
                }
            }
        }

        private readonly List<SpellSource> _spellSource = new List<SpellSource>();
    }
}
