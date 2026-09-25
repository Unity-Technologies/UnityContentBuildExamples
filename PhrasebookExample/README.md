# Phrasebook Example

This is an example of a single [content directory](https://docs.unity3d.com/Manual/content-directories.html) with Localized images.

It is an example of a simple pattern for "variants" when using a content directory build.

The example is based on Unity 6.6.

The scene shows four greeting cards. A dropdown selects English, French or Spanish, and every card
swaps to the image for that language. All twelve images are built into one content directory, and the
player loads only the ones for the selected language.

It shows a method to ship the same content in several variants and pick one at runtime.  This is similar to the AssetBundle variant functionality.  There is no build-time or file-name magic
involved: the variants are ordinary data on a `ScriptableObject`, referenced through `Loadable<T>`.

| English | French | Spanish |
|---|---|---|
| ![Hello](Assets/Cards/Hello/Hello.en.png) | ![Bonjour](Assets/Cards/Hello/Hello.fr.png) | ![Hola](Assets/Cards/Hello/Hello.es.png) |

## Usage

Open `Assets/Scenes/Phrasebook.unity` and press Play. Pick a language from the dropdown.

To build a player, select **Example > Build Player**. The player is built for the active platform into
`Build/Player`, and the content directory is built and copied into its `StreamingAssets` folder as
part of that build.

## How the pieces fit together

```mermaid
flowchart LR
    ROOT["PhrasebookCatalog<br/>(root asset)"] -. "Loadable&lt;GameObject&gt;" .-> P["HelloCard prefab<br/>Image + LocalizedImage"]
    P -->|direct reference| LS["Hello.asset<br/>(LocalizedSprite)<br/>en / fr / es"]
    LS -. "Loadable&lt;Sprite&gt;" .-> EN["Hello.en.png"]
    LS -. "Loadable&lt;Sprite&gt;" .-> FR["Hello.fr.png"]
    LS -. "Loadable&lt;Sprite&gt;" .-> ES["Hello.es.png"]
    LS ==>|"Resolve(fr).Load()"| FR
```

* **`LocalizedSprite`** is a `ScriptableObject` with a dictionary from language code to
  `Loadable<Sprite>`. One asset exists per phrase. Because the entries are loadable references, the
  build includes every language's sprite, but loading the asset loads none of them.
* **`LocalizedImage`** sits next to an `Image` on each card prefab and holds a direct reference to the
  phrase's `LocalizedSprite`. When it is enabled, and whenever the language changes, it resolves the
  entry for the current language, loads that one sprite, shows it, and releases the previous one.
* **`PhrasebookCatalog`** is the root asset passed to `BuildContentDirectory`. It lists the card prefabs
  as `Loadable<GameObject>`. The build follows the references from there, so the whole phrasebook is
  built from this one asset.

Dotted lines are on-demand references and solid lines are direct references. A direct reference loads
with the object that holds it, which is fine for the small `LocalizedSprite` asset. A loadable
reference is built but only loaded when code asks for it, which is what keeps the other languages out
of memory.

## Play mode and the player

The example does not need a content build to run in the Editor. In Play mode, `CatalogProvider` loads
`PhrasebookCatalog.asset` from the project, and the `Loadable` fields reached from it resolve to the
project's assets. In a player it registers the content directory from `StreamingAssets` and reads the
catalog from there with `ContentLoadManager.GetRootAssets`. Nothing else in the code knows the
difference.

The [AudioExample](../AudioExample/) shows the other approach, where Play mode registers a content
directory that was built beforehand, so that the Editor and the player exercise the same code path.

## Project content

* `Assets/Cards/<Phrase>/` holds the images for one phrase, named `<Phrase>.<lang>.png`, and the
  phrase's `LocalizedSprite` asset, `<Phrase>.asset`. The language is a lower-case two-letter code.
* `Assets/Prefabs/` holds one card prefab per phrase: an `Image` with a `LocalizedImage`.
* `Assets/RootAssets/PhrasebookCatalog.asset` is the root asset of the content build.
* `Assets/Scenes/Phrasebook.unity` is the only scene in the player. It holds the UI and the
  `PhrasebookController`.
* `Assets/Scripts/`
  * `LanguageSetting.cs`: the current language, the list of languages, and a change event.
  * `LocalizedSprite.cs`, `LocalizedImage.cs`, `PhrasebookCatalog.cs`: the three types described above.
  * `CatalogProvider.cs`: returns the catalog, from the project in Play mode or from the content
    directory in a player.
  * `PhrasebookController.cs`: fills the dropdown and instantiates the cards.
* `Assets/Editor/`
  * `LocalizedSpriteSync.cs`: an `AssetPostprocessor` that fills each `LocalizedSprite` from the images
    in its folder whenever they change, and warns when a language is missing. It only runs when an
    image is imported, so **Example > Sync Localized Sprites** exists for the one change it cannot
    see: editing the language list in `LanguageSetting`.
  * `CardTextureImporter.cs`: imports the card images as sprites.
  * `BuildAll.cs`: the **Example** menu items.
  * `ContentDirectoryDeployment.cs`: a `BuildPlayerProcessor` that builds the content directory during
    the player build and adds it to `StreamingAssets`.
* `Tools/New-PhraseCards.ps1` generated the card images. It is only needed to change a word or add a
  language; the images are committed.

## Adding a phrase or a language

To add a phrase, create `Assets/Cards/<Phrase>/` with a `<Phrase>.<lang>.png` for each language.
The sync creates `<Phrase>.asset`. Then make a card prefab for it and add the prefab to
`PhrasebookCatalog`.

To add a language, add its code and name to `LanguageSetting.Available`, then select
**Example > Sync Localized Sprites**. The Console warns for every phrase that has no image for the new
language, and those cards fall back to English until you add a `<Phrase>.<lang>.png` to each folder.
Adding the images updates the `LocalizedSprite` assets automatically.

## Other ways to organize variants

This example keys the data by phrase, then by language: one `LocalizedSprite` per phrase holds every
language. That suits a prefab that shows one piece of content, because the prefab needs a single
reference regardless of the language. There are other valid approaches. Content directories place
no requirement on how variants are organized; anything reachable from a root asset through direct or
loadable references is built, and any code you write can pick which reference to load.

Some alternatives, each a reasonable fit for a different situation:

* **Language first, then phrase.** A `LanguagePack` asset per language holds a dictionary from phrase
  key to `Loadable<Sprite>`. A translation team can deliver a whole language as one folder, and a
  postprocessor can fill the pack from it. Code looks up the current language's pack, then the phrase.
  A prefab then refers to the phrase by key rather than by a direct reference, so a missing key shows
  up at runtime rather than in the Inspector.
* **One content directory per language.** With the language-first layout, each `LanguagePack` can be
  the root asset of its own content directory. The player ships with one language and registers others
  when they are present, and `ContentLoadManager.GetRootAssets<LanguagePack>()` returns whichever are
  registered. The [AudioExample](../AudioExample/) shows several content directories working together.
* **Variants of the same asset rather than different assets.** When the variants are the same source
  asset at different import settings, for example texture quality tiers, the loadable id is the same in
  every build. A high quality content directory registered after the base one takes precedence for
  that id, so `Loadable<T>` fields resolve to the higher quality version without any table.  This is similar to how AssetBundle variants work, but at the scale of an entire content directory build.
* **A single lookup table.** A root asset with a dictionary from a composite key such as
  `"Hello/fr"` to `Loadable<Sprite>` is the smallest possible structure, and is close to loading by
  string from an AssetBundle. It works, but the Inspector cannot show which languages a phrase has,
  and nothing warns when one is missing.

Whichever layout you choose, the parts that matter are the same: every variant is behind a
`Loadable<T>` so that only the chosen one is loaded, and the choice is driven by a single place in the code.

## Concepts demonstrated

* `Loadable<T>` values inside a serialized `Dictionary`, so one asset describes every variant of a
  piece of content.
* A prefab that references a small `ScriptableObject` instead of a sprite, so the prefab is not tied to
  one language and loading it does not load any image.
* Resolving a variant at runtime and releasing the previous one, with the choice kept in one place.
* A single root asset that pulls the whole phrasebook into the content directory build through
  references, with no list of files to maintain.
* An `AssetPostprocessor` that keeps the data assets in step with a folder of images and reports gaps.
* `BuildPlayerProcessor.PrepareForBuild` building the content directory for the active platform inside
  the player build, and `AddAdditionalPathToStreamingAssets` shipping it with the player.
* Running in Play mode from the project's assets, with no content build required.

## Additional resources

* [Use content directories to load assets at runtime](https://docs.unity3d.com/Manual/content-directories.html)
* [Create content directories](https://docs.unity3d.com/Manual/content-directories-create.html)
* [Reference content in a content directory](https://docs.unity3d.com/Manual/content-directories-references.html)
* [Load content directories](https://docs.unity3d.com/Manual/content-directories-load.html)
