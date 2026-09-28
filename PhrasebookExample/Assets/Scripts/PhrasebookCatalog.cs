using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;

// The root asset of the content directory build.  It lists the card prefabs; each prefab references
// its LocalizedSprite, which references every language's sprite, so this one asset pulls the whole
// phrasebook into the build.
[CreateAssetMenu(fileName = "PhrasebookCatalog", menuName = "Scriptable Objects/Phrasebook Catalog")]
public class PhrasebookCatalog : ScriptableObject
{
    public List<Loadable<GameObject>> cards = new();
}
