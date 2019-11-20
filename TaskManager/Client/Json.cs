using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Client
{
    class Json
    {
        private string FilePath; 
        public Json()
        {
            FilePath = System.AppContext.BaseDirectory + "../../../Resources/Rules/rules.json";
        }

        public void AddToJsonRuleList(Rule rule)
        {
            JArray RulesArray;
            using (StreamReader r = new StreamReader(FilePath))
            {
                string json = r.ReadToEnd();
                RulesArray = JArray.Parse(json); // Va chercher la liste storée en JSON. 
            }

            RulesArray.Add(JToken.Parse(JsonConvert.SerializeObject(rule))); // Ajoute la nouvelle règle dans le tableau. 

            AddToFile(FilePath, RulesArray); 
        }

        private void AddToFile(string FilePath, JArray Array)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter(FilePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, Array); // Ajoute le nouveau tableau en Json dans le fichier rules.json. 
            }
        }

        public ObservableCollection<Rule> GetRulesListFromJson()
        {
            JArray RulesArray;
            ObservableCollection<Rule> rules = new ObservableCollection<Rule>();
            using (StreamReader r = new StreamReader(FilePath))
            {
                string json = r.ReadToEnd();
                RulesArray = JArray.Parse(json);
            }
            if (!(RulesArray is null))
            {
                foreach (var item in RulesArray.Children())
                {
                    rules.Add(JsonConvert.DeserializeObject<Rule>(item.ToString()));
                }
            }
            return rules;
        }

        public void UpdateJsonList(ObservableCollection<Rule> rulesList)
        {
            JArray RulesArray = new JArray();
            foreach (Rule r in rulesList)
            {
                RulesArray.Add(JToken.Parse(JsonConvert.SerializeObject(r)));
            }
            AddToFile(FilePath, RulesArray); 
        }
    }
}
