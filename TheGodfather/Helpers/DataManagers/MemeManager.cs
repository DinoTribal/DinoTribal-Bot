﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers.DataManagers
{
    public class MemeManager
    {
        public IReadOnlyDictionary<string, string> Memes => _memes;
        public readonly string TemplateDirectory = "Resources/meme-templates/";
        private ConcurrentDictionary<string, string> _memes = new ConcurrentDictionary<string, string>();
        private bool _ioerr = false;


        public MemeManager()
        {

        }


        public void Load()
        {
            if (File.Exists("Resources/memes.json")) {
                try {
                    _memes = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(File.ReadAllText("Resources/memes.json"));
                } catch (Exception e) {
                    Console.WriteLine("Meme loading error, check file formatting. Details:\n" + e.ToString());
                    _ioerr = true;
                }
            } else {
                Console.WriteLine("memes.json is missing.");
            }
        }

        public bool Save()
        {
            if (_ioerr) {
                Console.WriteLine("Meme saving skipped until file conflicts are resolved!");
                return false;
            }

            try {
                File.WriteAllText("Resources/memes.json", JsonConvert.SerializeObject(_memes, Formatting.Indented));
            } catch (Exception e) {
                Console.WriteLine("Meme save error. Details:\n" + e.ToString());
                return false;
            }

            return true;
        }

        public string GetRandomMeme()
        {
            if (_memes.Count == 0)
                return null;
            
            List<string> names = new List<string>(_memes.Keys);
            return _memes[names[new Random().Next(names.Count)]];
        }

        public bool Contains(string name)
        {
            return _memes.ContainsKey(name.ToLower());
        }

        public string GetUrl(string name)
        {
            name = name.ToLower();
            if (_memes.ContainsKey(name))
                return _memes[name];
            else
                return null;
        }

        public bool TryAdd(string name, string url)
        {
            name = name.ToLower();

            if (_memes.ContainsKey(name))
                return false;

            if (!_memes.TryAdd(name, url))
                return false;

            return true;
        }

        public bool TryRemove(string name)
        {
            name = name.ToLower();
            if (!_memes.ContainsKey(name))
                return false;

            return _memes.TryRemove(name, out _);
        }

        public void ClearAllMemes()
        {
            _memes.Clear();
        }

        public IReadOnlyList<string> GetAllTemplateNames()
        {
            var templates = Directory.GetFiles(TemplateDirectory)
                .Select(s => s.Substring(TemplateDirectory.Length, s.Length - TemplateDirectory.Length - 4))
                .ToList();
            templates.Sort();
            return templates;
        }
    }
}
