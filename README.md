# <img src="Assets/Graphics/app-icon.png" width="45" valign="top" /> STL Vault
STL Vault is an open source 3d model viewer and organizer.
Think of it as Lightroom, but for 3d printing.

## Releases
STL Vault is still under active development and has no stable release yet.
You'll be able to download versions for Windows, Linux and Mac here in the future.

## Roadmap
Here's whats currently planned for STL Vault:

### Version 1.0 (Q1 2020)
This version is currently under development

- [x] Import .stl files from folders
  - [x] Automatic tagging
  - [x] Automatic rotation
  - [x] Automatic scaling
- [x] Generate previews for imported files
- [x] Allow to search for tagged items
- [x] Save previous searches
- [ ] User defined collections of items
- [ ] Basic Operations (Single file + Batch)
  - [ ] Allow editing of tags
  - [ ] Non-Destructive Editing
    - [ ] Scale (Resize)
    - [ ] Rotate
- [ ] Export for printing

### Version 2.0 (Q2 2020)
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
For quick feedback or to chat with me directly, 
you can join the [discord](https://discord.gg/Fd4n3v).

If you want to interact here:

### You are an end user and ...

#### ... you are missing a feature
If the feature is on the roadmap, feel free to head to the corresponding issue.
You can leave your upvote, discuss details and give me more information on what you need.

If the feature isn't on the roadmap, browse/search the issues to see if someone else has already requested it.
If that isn't the case - feel free to open a new issue to let me know what you need.

#### ... STL-Vault crashed on your system
Yikes, sorry! You can help by reporting the crash via a new issue.
Attach the `Player.log` found in `%USERPROFILE%\AppData\LocalLow\StlVault\StlVault` to the issue.

#### ... STL-Vault didn't process a file correctly
You can open an issue with a link to the file if it is _openly available_. 
Do _not_ attach files to issues. If the file isn't openly available, 
there is not a lot I can do right now.
Don't share files without the authors permission!

### You want to work on this project
Fist of all: Thanks, that's awesome!

I'll accept pull request if they provide value for a good part of the user base.
Opening an issue to discuss changes beforehand doesn't hurt either.
