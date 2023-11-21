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
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Http;
using UnityEngine;
using BestHTTP;
using Random = UnityEngine.Random;

namespace atmHack
{
    public class atmHack : Plugin
    {
        private LifeServer _server;
        private long _lastRob;
        private static string _dirPath;
        public static string ConfPath;
        private atmhackConfig _config;

        private logsLSPLUGIN _logs;
        private int policeCount;

        public atmHack(IGameAPI api)
          : base(api)
        {
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            this.InitDirectory();
            this._server = Nova.server;
            this._server.OnPlayerTryToHackATM += new Action<Player>(this.TryHack);
            SChatCommand schatCommand1 = new SChatCommand("/atm", "Permet d'afficher les commandes d'atm", "/atm", (Action<Player, string[]>)((player, arg) =>
            {
                if (player.IsAdmin)
                {

                    UIPanel atmmenu = new UIPanel("Hack Tool menu", UIPanel.PanelType.Tab);
                    atmmenu.AddTabLine("Recharger le fichier de configuration", (ui) => { this._config = JsonConvert.DeserializeObject<atmhackConfig>(File.ReadAllText(atmHack.ConfPath)); player.SendText("<color=green>Rechargement OK</color>"); });
                    atmmenu.AddTabLine("Rénitialiser le cooldown", (ui) => { this._lastRob = 0L; player.SendText("<color=green>Le cooldown vient d'être rénitialiser</color>"); });
                    atmmenu.AddTabLine("Donner un ATM (posable)", (ui) => { player.setup.inventory.AddItem(58, 1, ""); player.SendText("<color=green>Vous avez reçu 1x ATM</color>"); });
                    atmmenu.AddButton("Fermer", (ui) => { player.ClosePanel(ui); });
                    atmmenu.AddButton("Sélectionner", (Action<UIPanel>)(ui1 => ui1.SelectTab()));
                    player.ShowPanelUI(atmmenu);


                }
                else
                    player.SendText("<color=red>Vous n'êtes pas administrateur !</color>");
            }));
            schatCommand1.Register();
        }

        private bool isActivity(Player player, Activity.Type type)
        {
            if (!player.HasBiz()) return false;

            foreach (Activity.Type activity in Nova.biz.GetBizActivities(player.biz.Id))
            {
                if (activity == type)
                {
                    return true;
                }
            }

            return false;
        }

        public void TryHack(Player player)
        {
            UIPanel uiPanel = new UIPanel("Hack Tool V0.4", (UIPanel.PanelType)0).AddButton("Commencer le Hack", (Action<UIPanel>)(ui =>
            {


                foreach (Player player2 in this._server.Players)
                {
                    if (player2.character != null)
                    {
                        if (player2.isInGame)
                        {
                            if (player2.HasBiz() == true)
                            {

                                if (isActivity(player2, Activity.Type.LawEnforcement))
                                {


                                    this.policeCount += 1;

                                }

                            }
                        }
                    }
                }
                if (this.policeCount < this._config.police)
                {
                    player.SendText("<color=#e8472a>Il n'y a pas assez de policiers pour braquer." + "" + "</color>");
                    policeCount = 0;
                    return;
                }

                if (this._lastRob > Nova.UnixTimeNow())
                {
                    player.SendText("<color=red>Veuillez attendre quelques heures avant de braquer ce DAB.</color > ");
                }
                else
                {
                    ((MonoBehaviour)this._server.lifeManager).StartCoroutine(this.GetMoney(player, ((UnityEngine.Component)player.setup).transform.position));
                    player.SendText("<color=red>HackTool.exe est en cours de lancement</color > ");
                    player.ClosePanel(ui);

                    if (this._logs.ATMWebhookURL != "")
                    {
                        string message = $"{player.steamId} - {player.GetFullName()} est entrain de braquer l'ATM";
                        //this.server.SendMessageToAll(message);
                        string payload = $"{{\"content\":\"{message}\"}}";
                        HTTPRequest request = new HTTPRequest(new Uri(this._logs.ATMWebhookURL), HTTPMethods.Post, (req, res) =>
                        {
                            if (res.IsSuccess)
                            {
                                UnityEngine.Debug.Log("Webhook envoyé ");
                            }
                            else
                            {
                                UnityEngine.Debug.LogError($"Erreur de l'envoi du webhook: {res.Message}");
                            }
                        });
                        request.AddHeader("Content-Type", "application/json");
                        request.RawData = System.Text.Encoding.UTF8.GetBytes(payload);
                        request.Send();

                        //Debug.LogError(message);
                    }
                }
            })).AddButton("Close", new Action<UIPanel>(player.ClosePanel)).SetText("Voulez vous lancer HackTool.exe ?");
            player.ShowPanelUI(uiPanel);
        }

        private void InitDirectory()
        {
            atmHack._dirPath = this.pluginsPath + "/atmHack";
            atmHack.ConfPath = atmHack._dirPath + "/config.json";
            if (!Directory.Exists(atmHack._dirPath))
                Directory.CreateDirectory(atmHack._dirPath);
            if (!File.Exists(atmHack.ConfPath))
            {
                this._config = new atmhackConfig()
                {
                    hackCooldown = 21600L,
                    minMoney = 1500,
                    maxMoney = 3000,
                    hackDuration = 20,
                    police = 2
                };
                string contents = JsonConvert.SerializeObject((object)this._config);
                File.WriteAllText(atmHack.ConfPath, contents);
            }
            else
            {
                string str = File.ReadAllText(atmHack.ConfPath);
                try
                {
                    this._config = new atmhackConfig();
                    this._config = JsonConvert.DeserializeObject<atmhackConfig>(str);
                    string contents = JsonConvert.SerializeObject((object)this._config);
                    File.WriteAllText(atmHack.ConfPath, contents);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log((object)ex.Message);
                    UnityEngine.Debug.LogException(ex);
                }
            }


            atmHack._dirPath = this.pluginsPath + "/LOGS";
            atmHack.ConfPath = atmHack._dirPath + "/ATMlogs.json";
            if (!Directory.Exists(atmHack._dirPath))
                Directory.CreateDirectory(atmHack._dirPath);
            if (!File.Exists(atmHack.ConfPath))
            {
                this._logs = new logsLSPLUGIN()
                {
                    ATMWebhookURL = "",
                };

                string contents = JsonConvert.SerializeObject((object)this._logs);
                File.WriteAllText(atmHack.ConfPath, contents);
            }
            else
            {
                string str = File.ReadAllText(atmHack.ConfPath);
                try
                {
                    this._logs = new logsLSPLUGIN();
                    this._logs = JsonConvert.DeserializeObject<logsLSPLUGIN>(str);
                    string contents = JsonConvert.SerializeObject((object)this._logs);
                    File.WriteAllText(atmHack.ConfPath, contents);
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
            //  foreach (Player playerpolice in ((IEnumerable<Player>)this._server.Players).Where<Player>((Func<Player, bool>)(p =>
            // {
            //    if (p.isInGame)
            // {
            // Characters character = p.character;
            //   Bizs bizs;
            //   try
            //   {
            //       bizs = ((IEnumerable<Bizs>)Nova.biz.bizs).First<Bizs>((Func<Bizs, bool>)(u => u.Id == character.BizId));
            //   }
            //    catch
            //   {
            //      return false;
            //   }
            //   if (bizs != null && character != null && bizs.IsActivity((Activity.Type.LawEnforcement)))
            //       return p.serviceMetier;
            // }
            //return false;
            //   })))

            foreach (Player player2 in this._server.Players)
            {
                if (player2.character != null)
                {
                    if (player2.isInGame)
                    {
                        if (player2.HasBiz() == true)
                        {

                            if (isActivity(player2, Activity.Type.LawEnforcement))
                            {


                                player2.setup.TargetPlayClairon(0.5f);
                                player2.SendText("<color=#e8472a>Un braquage de DAB est en cours, un point a été placé sur votre carte.</color>");
                                player2.setup.TargetSetGPSTarget(position);
                                player2.SendText("<color=#e8472a>La caméra a identifié un individu, les résultats peuvent être inexacts :" + player.GetFullName().Replace("A", "$").Replace("E", "%").Replace("I", "^") + " </color> ");


                            }

                        }
                    }
                }
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
            player.AddMoney((double)+num, "ATM ROB");
            if (i == 60)
                player.SendText("Braquage terminé");
            player.SendText(string.Format("<color=#e8472a>Vous avez volé {0}€</color>", (object)num));
        }
    }
}
