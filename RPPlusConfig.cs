using Newtonsoft.Json;
using System;
using System.IO;

namespace RPPlus
{
    [Serializable]
    public class RPPlusConfig
    {
        public long hackCooldown;
        public int minMoney;
        public int maxMoney;
        public int hackDuration;

        public void Save()
        {
            string contents = JsonConvert.SerializeObject((object)this);
            File.WriteAllText(RoleplayPlus.ConfPath, contents);
        }
    }
}
