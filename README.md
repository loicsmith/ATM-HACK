
# PLUGIN ATM-HACK

Plugin de braquage d'atm sur le jeu Nova-Life : Amboise

# Contact

En cas de problème/questions, vous pouvez me contactez sur discord : loicsmith (ou Loicsmith#0275)


## Installation
1. Télecharger le plugin (le .dll) : https://github.com/loicsmith/ATM-HACK/releases/tag/ATM-HACK
2. Placer le dans votre dossier plugins de votre serveur
3. Démarrer votre serveur
4. Un dossier AtmHack et logs viennent de se crée
5. dans le dossier AtmHack, il y a un fichier, ouvrez le et configurer le comme bon vous semble. (voir Notes pour comprendre comment configurer). Pour le dossier logs, à l'intérieur il y a un fichier, ouvrez le et mettez simplement un webhook discord valide


## Fonctionalités

- Braquage d'atm
- Configurable (argent, temps avant chaque braquage, durée du braquage, nombre de policier requis)
- Menu pour rénitialiser le temps avant chaque braquage, et pour recharger le fichier de configuration sans redémarré le serveur !
- Système de logs par webhook discord ! (Pour l'installation, un fichier json apparaitra directement dans le dossier plugins > logs)

## Notes

- pour la configuration du min / max argent obtensible, veuillez diviser par la durée du braquage, le min/max est l'argent gagné par seconde !
  
## Commandes

/atm : permet d'afficher le menu permettant de rénitialiser le cooldown après braquage, de recharger le fichier de configuration (si modification de celui, pas besoin de redémarrer le serveur) et de se donner 1x ATM (posable)  
