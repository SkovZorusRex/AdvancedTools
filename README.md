# AdvancedTools

Il y a des bugs lors d'ajout d'objet ou bien lors du changement de Scene, cela est dû à la manière dont l'UI est fait.
Je n'ai pas utilisé le GUI mais le nouveau système d'UI (https://docs.unity3d.com/Manual/UIElements.html) sans pour autant faire de l'uxml et de l'uss.
Ce qui fait que je ne comprend pas comment faire fonctionner correctement certains elements (ListView) d'où les bugs ...

Features supplémentaires
- ~~Pouvoir accélérer ou ralentir la vitesse de l’animation simulée.~~                                        **FAIT**
- ~~Pouvoir sampler l’animation simulée via un slider.~~                                                      **FAIT**
- ~~Afficher des informations supplémentaires sur l’animation sélectionnée~~                                  **FAIT**
- Pouvoir rajouter un délai configurable entre deux répétitions de l’animation simulée.
- ~~Mettre en valeur l’Animator sélectionné dans la Hierarchy~~                                               **FAIT**
- Pouvoir sélectionner les Animators et les Animations avec un autre composant que la liste
déroulante.
- Barre de recherche pour filtrer les Animator de la scène.
- ~~Remettre la liste des Animator à jour quand un objet est rajouté/supprimé dans la Hierarchy~~             **FAIT**
- ~~Remettre la liste des Animator à jour quand la scène change.~~                                            **FAIT**
- Rajouter des options de configuration et de données persistantes pour le simulator via un
ScriptableObject.
- Afficher uniquement les Animator actifs.
- Enregistrer les délais de répétition par animation.
- Pouvoir jouer plusieurs animations en même temps.
