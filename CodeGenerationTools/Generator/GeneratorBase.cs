using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTools.Generator
{
    public abstract class GeneratorBase<T> where T : TemplateConfigBase
    {
        protected string _tmpd;
        protected T _config;

        public void LoadTemplate(string template)
        {
            _tmpd = template;
        }

        public void LoadTemplateFromFile(string filePath)
        {
            LoadTemplate(File.ReadAllText(filePath));
        }

        public void SetConfig(T config)
        {
            _config = config;
        }

        private void Replace(string keyword, string content)
        {
            _tmpd = _tmpd.Replace(keyword, content);
        }

        public string Generate()
        {
            if (_config == null)
            {
                Console.WriteLine("Template Config is null,please use SetConfig to Init Config");
                return null;
            }

            if (_tmpd == null)
            {
                Console.WriteLine("Template  is null,please use LoadTemplate to init template");
                return null;
            }

            foreach (var key in _config.KeyWordList)
            {
                Replace(key, _config.GetContent(key));
            }

            return _tmpd;
        }
    }
}
