# <img src="Assets/Graphics/app-icon.png" width="45" valign="top" /> STLVault
STLVault is an open source 3d model viewer and organizer.
Think of it as Lightroom, but for 3d printing.

## Releases
STLVault is still under active development and has no stable release yet.
When released there will be builds for Windows, MacOS and Linux.

Currently, you can download previews in the [releases section](https://github.com/rubenwe/StlVault/releases).

## Roadmap
Here's whats currently planned for STL Vault:

### Version 1.0 (currently under development)
This version is currently under development

- [x] Import .stl files from folders
  - [x] Automatic tagging
  - [x] Automatic rotation
  - [x] Automatic scaling
- [x] Generate previews for imported files
- [x] Allow to search for tagged items
- [x] Save previous searches
- [x] 3D View to preview items
- [x] Basic Operations (Single file + Batch)
  - [x] Allow editing of tags
  - [x] Non-Destructive Editing
    - [x] Rotate
- [x] User defined collections of items
- [ ] Export for printing

### Version 2.0 (Q2/Q3 2020)
- [ ] Allow Hierarchies
  - [ ] Nested Collections
  - [ ] File System Structure
- [ ] Automatic backups
  - [ ] Denser storage format .vault
  - [ ] Define folder structure based on tags
  - [ ] To local drives/folders
  - [ ] To cloud providers
- [ ] Community tagging
  - [ ] Allow users to share tags
  - [ ] Allow users to consume tags
- [ ] Handle multi part models
  - [ ] Loose collections of parts
  - [ ] Assemble to one model
  - [ ] Variation handling (for modular models)
- [ ] Import more formats
  - [ ] .3mf
  - [ ] .fbx
  - [ ] .obj
- [ ] Export more formats
  - [ ] .3mf

### Future releases
- [ ] Automatic rigging of models for reposing
- [ ] Base-Generator


## Contributing
For quick feedback or to chat with the developer(s) directly,
you can join the [discord](https://discord.gg/sexQM8R).

If you want to interact here:

### You are an end user and ...

#### ... you are missing a feature
If the feature is on the roadmap, feel free to head to the corresponding issue.
You can leave your upvote, discuss details and give me more information on what you need.

If the feature isn't on the roadmap, browse/search the issues to see if someone else has already requested it.
If that isn't the case - feel free to open a new issue to let me know what you need.

#### ... STL-Vault crashed on your system
Yikes, sorry! You can help by reporting the crash:
- If you can still open STLVault, use the `(!)` button and create a crash archive.
- Submit the archive either by:
  - create a new issue for your crash and attach the zip
  - join the [discord](https://discord.gg/sexQM8R) and report there

#### ... STL-Vault didn't process a file correctly
Same as reporting a crash, but to speed up the process:
- include a link to the file if it is _openly available_
- do _not_ attach files to issues

Do _not_ share files against their license or without the authors permission!

### You want to work on this project
Fist of all: Thanks, that's awesome!

I'll accept pull request if they provide value for a good part of the user base.
Opening an issue or chatting with the developer(s) in the discord to discuss changes 
beforehand doesn't hurt either.

If you want to play around with the sources on your own look at the [Developer docs](DEVELOPMENT.md).