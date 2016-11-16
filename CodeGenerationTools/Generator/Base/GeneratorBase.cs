using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeGenerationTools.Generator.Base
{
    public abstract class GeneratorBase<TData> : IGenerator
    {
        private readonly Regex _regex = new Regex("\\{\\$(?:[a-z][a-z0-9_]*)\\}", RegexOptions.IgnoreCase | RegexOptions.Singleline);// 

        protected string Template;
        protected TData Data;

        protected string GenTmpad;
        protected HashSet<string> KeyWordList = new HashSet<string>();
        protected Dictionary<string, object> KeyDictionary = new Dictionary<string, object>();


        public virtual bool LoadTemplate(string template)
        {
            Template = template;

            KeyWordList.Clear();
            var m = _regex.Match(Template);
            while (m.Success)
            {
                if (!KeyWordList.Contains(m.Value))
                {
                    KeyWordList.Add(m.Value);
                }
                m = m.NextMatch();
            }

            return !string.IsNullOrEmpty(Template);
        }

        public virtual bool LoadTemplateFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
            LoadTemplate(File.ReadAllText(filePath));
            return true;
        }

        public abstract bool LoadData(TData data);

        protected void SetKeyValue(string key, object content)
        {
            if (!KeyWordList.Contains(key))
            {
                Console.WriteLine("Invalid key word");
                return;
            }

            if (KeyDictionary.ContainsKey(key))
                KeyDictionary[key] = content;
            else
                KeyDictionary.Add(key, content);
        }
        
        private string GetContent(string key)
        {
            var content = KeyDictionary[key];
            var s = content as string;
            if (s != null)
                return s;
            var generator = content as IGenerator;
            return generator?.Generate();
        }

        private void Replace(string keyword, string content)
        {
            GenTmpad = GenTmpad.Replace(keyword, content);
        }

        public bool Init(string template, TData data)
        {
            if (!LoadTemplate(template))
                return false;
            if (!LoadData(data))
                return false;
            return true;
        }

        public bool InitFromFile(string tmpdFilePath, TData data)
        {
            if (!LoadTemplateFromFile(tmpdFilePath))
                return false;
            if (!LoadData(data))
                return false;
            return true;
        }

        public string Generate()
        {
            if (string.IsNullOrEmpty(Template))
            {
                Console.WriteLine("{0}'s Template  is null,please use LoadTemplate to init template", GetType().Name);
                return null;
            }

            GenTmpad = Template;

            foreach (var key in KeyWordList)
            {
                Replace(key, GetContent(key));
            }

            return GenTmpad;
        }

    }
}
