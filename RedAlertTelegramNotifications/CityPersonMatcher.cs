using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedAlertTelegramNotifications
{
    public class CityPersonMatcher
    {
        private const string CITIES_FILE_NAME = "districtNames.txt";
        private const string PEOPLE_FILE_NAME = "peopleLocations.txt";

        private readonly Dictionary<string, List<string>> cityPersonDic;

        public CityPersonMatcher()
        {
            cityPersonDic = new();
        }

        public void InitializeMatcherData()
        {
            InitializeCities();
            InitializePeople();
        }

        private void InitializeCities()
        {
            var cities = File.ReadAllLines(CITIES_FILE_NAME);

            foreach(var city in cities)
            {
                cityPersonDic.Add(city, new List<string>());
            }
        }

        private void InitializePeople()
        {
            var people = File.ReadAllLines(PEOPLE_FILE_NAME);

            foreach(var personData in people)
            {
                var person = personData.Split('|', StringSplitOptions.TrimEntries);
                var name = person[0];
                var cities = person.Skip(1);

                foreach(var city in cities)
                {
                    cityPersonDic[city].Add(name);
                }
            }
        }

        public List<string> GetPeople(string[] cities)
        {
            return cityPersonDic.Where(p => cities.Contains(p.Key)).SelectMany(p => p.Value).ToList();
        }
    }
}
