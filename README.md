# Unity Content Build Examples

Small Unity projects that show how to build and load content with the content build features of
Unity 6.6 and later, such as [content directories](https://docs.unity3d.com/Manual/content-directories.html).
Each example is a complete project in its own folder, with a README that explains what it demonstrates and
how to run it.

| Example                        | Unity version | Demonstrates                                                          |
| ------------------------------ | ------------- | --------------------------------------------------------------------- |
| [AudioExample](AudioExample/)  | 6000.6        | Splitting audio into a Core content directory and optional Sound Packs, root assets, `Loadable<T>`, and deploying content directory builds into a player |
| [PhrasebookExample](PhrasebookExample/) | 6000.6 | Localized images selected at runtime from one content directory: `Loadable<T>` in a serialized dictionary, a prefab component that swaps the variant, Play mode without a content build. A replacement for AssetBundle variants |


## Maintenance

This repository is maintained by the Unity Content Build & Distribution team.

## Scope and versions

Each example targets a specific Unity version, listed in the table above, and is kept working on that version. 

Examples are updated when the content build APIs they demonstrate change in a way that breaks them, or upgraded if the targeting Unity version reaches the end of its support life cycle.

Examples that demonstrate a removed or superseded feature will be retired.

## Contributions and feedback.

This repository is published as read-only reference material, so pull requests are not accepted.

Issues are welcome — please report examples that do not work as described, or that no longer build on the described Unity version (or a more recent version).  Feedback can also be posted to [Unity Discussions](https://discussions.unity.com).

## License

Licensed under the Unity Companion License for Unity-dependent projects.  See [LICENSE.md](LICENSE.md).
