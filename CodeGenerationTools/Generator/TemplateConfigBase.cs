using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTools.Generator
{
    public abstract class TemplateConfigBase
    {
        public abstract List<string> KeyWordList { get; }

        protected Dictionary<string, string> KeyDictionary = new Dictionary<string, string>();
        
        private TemplateConfigBase()
        {

        }

        internal string GetContent(string key)
        {
            string content;
            return KeyDictionary.TryGetValue(key, out content) ? content : null;
        }

        public void SetKeyValue(string key, string content)
        {
            if (KeyDictionary.ContainsKey(key))
                KeyDictionary[key] = content;
            else
                KeyDictionary.Add(key, content);
        }

        public void Clear()
        {
            KeyDictionary.Clear();
        }
    }
}
