using Life;
using Life.BizSystem;
using Life.DB;
using Life.Network;
using Life.UI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RPPlus
{
    public class RoleplayPlus : Plugin
    {
        private LifeServer _server;
        private long _lastRob;
        private static string _dirPath;
        public static string ConfPath;
        private RPPlusConfig _config;

        public RoleplayPlus(IGameAPI api)
          : base(api)
        {
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            this.InitDirectory();
            this._server = Nova.server;
            this._server.OnPlayerTryToHackATM += new Action<Player>(this.TryHack);
            SChatCommand schatCommand1 = new SChatCommand("/reloadatm", "Permet de recharger le plugin", "/reloadatm", (Action<Player, string[]>)((player, arg) =>
   {
            if (player.IsAdmin)
                {
                    this._config = JsonConvert.DeserializeObject<RPPlusConfig>(File.ReadAllText(RoleplayPlus.ConfPath));
                    player.SendText("<color=green>Rechargement OK</color>");
                }
                else
                    player.SendText("<color=red>Vous n'êtes pas administrateur !</color>");
            }));
            SChatCommand schatCommand2 = new SChatCommand("/resetcooldownatm", "Permet de rénitaliser le cooldown pour braquer l'atm", "/resetcooldownatm", (Action<Player, string[]>)((player, arg) =>
            {
                if (player.IsAdmin)
                {
                    this._lastRob = 0L;
                    player.SendText("<color=green>Le cooldown vient d'être rénitialiser</color>");
                }
                else
                    player.SendText("<color=red>Vous n'êtes pas administrateur !</color>");
            }));

            SChatCommand schatCommand3 = new SChatCommand("/atm", "Permet d'afficher les commandes d'atm", "/atm", (Action<Player, string[]>)((player, arg) =>
            {
                if (player.IsAdmin)
                {

                    UIPanel atmmenu = new UIPanel("Hack Tool menu", UIPanel.PanelType.Tab);
                    atmmenu.AddTabLine("Recharger le fichier de configuration", (ui) => { this._config = JsonConvert.DeserializeObject<RPPlusConfig>(File.ReadAllText(RoleplayPlus.ConfPath)); player.SendText("<color=green>Rechargement OK</color>"); });
                    atmmenu.AddTabLine("Rénitialiser le cooldown", (ui) => { this._lastRob = 0L; player.SendText("<color=green>Le cooldown vient d'être rénitialiser</color>"); });
                    atmmenu.AddButton("Fermer", (ui) => { player.ClosePanel(ui); });
                    atmmenu.AddButton("Sélectionner", (Action<UIPanel>)(ui1 => ui1.SelectTab()));
                    player.ShowPanelUI(atmmenu);


                }
                else
                    player.SendText("<color=red>Vous n'êtes pas administrateur !</color>");
            }));
            schatCommand1.Register();
            schatCommand2.Register();
            schatCommand3.Register();
        }

        public void TryHack(Player player)
        {
            UIPanel uiPanel = new UIPanel("Hack Tool V0.4", (UIPanel.PanelType)0).AddButton("Commencer le Hack", (Action<UIPanel>)(ui =>
            {
                if (this._lastRob > Nova.UnixTimeNow())
                {
                    player.SendText("<color=red>Veuillez attendre quelques heures avant de braquer ce DAB.</color > ");
                }
                else
                {
                    ((MonoBehaviour)this._server.lifeManager).StartCoroutine(this.GetMoney(player, ((UnityEngine.Component)player.setup).transform.position));
                    player.SendText("<color=red>HackTool.exe est en cours de lancement</color > ");
                    player.ClosePanel(ui);
                }
            })).AddButton("Close", new Action<UIPanel>(player.ClosePanel)).SetText("Voulez vous lancer HackTool.exe ?");
            player.ShowPanelUI(uiPanel);
        }

        private void InitDirectory()
        {
            RoleplayPlus._dirPath = this.pluginsPath + "/RPPlus";
            RoleplayPlus.ConfPath = RoleplayPlus._dirPath + "/config.json";
            if (!Directory.Exists(RoleplayPlus._dirPath))
                Directory.CreateDirectory(RoleplayPlus._dirPath);
            if (!File.Exists(RoleplayPlus.ConfPath))
            {
                this._config = new RPPlusConfig()
                {
                    hackCooldown = 21600L,
                    minMoney = 1500,
                    maxMoney = 3000,
                    hackDuration = 20
                };
                string contents = JsonConvert.SerializeObject((object)this._config);
                File.WriteAllText(RoleplayPlus.ConfPath, contents);
            }
            else
            {
                string str = File.ReadAllText(RoleplayPlus.ConfPath);
                try
                {
                    this._config = new RPPlusConfig();
                    this._config = JsonConvert.DeserializeObject<RPPlusConfig>(str);
                    string contents = JsonConvert.SerializeObject((object)this._config);
                    File.WriteAllText(RoleplayPlus.ConfPath, contents);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log((object)ex.Message);
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }
        private IEnumerator GetMoney(Player player, Vector3 position)
        {
            foreach (Player player1 in ((IEnumerable<Player>)this._server.Players).Where<Player>((Func<Player, bool>)(p =>
            {
                if (p.isInGame)
                {
                    Characters character = p.character;
                    Bizs bizs;
                    try
                    {
                        bizs = ((IEnumerable<Bizs>)Nova.biz.bizs).First<Bizs>((Func<Bizs, bool>)(u => u.Id == character.BizId));
                    }
                    catch
                    {
                        return false;
                    }
                    if (bizs != null && character != null && bizs.IsActivity((Activity.Type)1))
                        return p.serviceMetier;
                }
                return false;
            })))
            {
                player1.setup.TargetPlayClairon(0.5f);
                player1.SendText("<color=#e8472a>Un braquage de DAB est en cours, un point a été placé sur votre carte.</color>");
                player1.setup.TargetSetGPSTarget(position);
                player1.SendText("<color=#e8472a>La caméra a identifié un individu, les résultats peuvent être inexacts :" + player.GetFullName().Replace("A", "$").Replace("E", "%").Replace("I", "^") + " </color> ");
            }
            this._lastRob = Nova.UnixTimeNow() + this._config.hackCooldown;
            int number = 0;
            int i;
            for (i = 0; i < this._config.hackDuration; ++i)
            {
                if ((double)Vector3.Distance(((UnityEngine.Component)player.setup).transform.position, position) < 4.0)
                {
                    ++number;
                    if (i == 0)
                    player.SendText("<color=#e8472a>Braquage en cours, il reste " + this._config.hackDuration.ToString() + " secondes</color>");
                    yield return (object)new WaitForSeconds(1f);
                }
                else
                {
                    player.SendText("<color=#e8472a>Braquage annulé.</color > ");
                    break;
                }
            }
            int num = Random.Range(this._config.minMoney, this._config.maxMoney) * number;
            player.AddMoney(num, "ATM ROB");
            if (i == 60)
                player.SendText("Braquage terminé");
            player.SendText(string.Format("<color=#e8472a>Vous avez volé {0}€</color>", (object)num));
        }
    }
}
