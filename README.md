# osu!progressCLI  
### THIS PROGRAMM IS IN EARLY ACCESS, BREAKING CHANGES ARE TO BE EXPECTED

-- add images and descripton here

## How to run
1. Download the zip
2. Extract it
3. run the osu!progressCLI.exe
4. Open ur Webbrowser and go to localhost:4200 (in cause its in use change it in the config.json add osu Songsfolder aswell (appears after first run)
5. CLick the Pen on the Website insert ur Clientid and ClientSecret and ur userid.
6. Hit save and refresh the Page ur Profile Stats should appear

## TODOs (no prioritys)

- [ ] extract js and css out of the page generation (server side serving needed)
- [ ] Add possiblity to reload api data incase no internet (or beatmap changed)  (automatic or manual)
- [x] localbeatmap parser (offline, not submitted maps support) (use osu-tools) (bpm still missing)
- [ ] highconfigurable website (showing what graphs u want and what not)
- [ ] page for a single score
- [ ] api endpoint for search functionaliy (add date functionaliy)
- [ ] make date picker work
- [x] mod support
- [ ] add local images in score displat website for mods and rankings (maybe rankedtype)
- [ ] importer for [circle-tracker](https://github.com/FunOrange/circle-tracker)

## NOTES
- rate limite api calls incause i ever make a importer (1 every 2 seconds) 
- change form .net 6.0 to somehting lower (to not have people install dependencys) (could nbot work cause osu tools is net6)
