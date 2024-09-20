﻿using Milimoe.FunGame.Core.Api.Utility;
using Milimoe.FunGame.Core.Entity;

namespace Milimoe.FunGame.Testing.Desktop.Solutions
{
    public class SkillManager
    {
        public Dictionary<string, Skill> LoadedSkills { get; set; } = [];

        public void Load()
        {
            EntityModuleConfig<Skill> config = new("EntityEditor", "skills");
            config.LoadConfig();
            LoadedSkills = new(config);
        }

        public bool Add(string name, Skill skill)
        {
            return LoadedSkills.TryAdd(name, skill);
        }

        public bool Remove(string name)
        {
            return LoadedSkills.Remove(name);
        }

        public void Save()
        {
            EntityModuleConfig<Skill> config = new("EntityEditor", "skills");
            foreach (string key in LoadedSkills.Keys)
            {
                config.Add(key, LoadedSkills[key]);
            }
            config.SaveConfig();
        }

        public void OpenCreator(Skill? skill = null)
        {
            SkillCreator manager = new(this, skill);
            manager.ShowDialog();
        }
    }
}
